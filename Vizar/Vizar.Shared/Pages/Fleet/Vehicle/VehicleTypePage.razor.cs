using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Vehicle;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Vehicle;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Vehicle;

public partial class VehicleTypePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private VehicleTypeModel _vehicleType = new();

    private List<VehicleTypeModel> _vehicleTypes = [];

    private SfGrid<VehicleTypeModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteVehicleTypeId = 0;
    private string _deleteVehicleTypeName = string.Empty;

    private int _recoverVehicleTypeId = 0;
    private string _recoverVehicleTypeName = string.Empty;

    private ToastNotification _toastNotification;

    #region Load Data
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, UserRoles.Admin);
        await LoadData();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadData()
    {
        _hotKeysContext = HotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.S, SaveVehicleType, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(TableNames.VehicleType);

        if (!_showDeleted)
            _vehicleTypes = [.. _vehicleTypes.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditVehicleType(VehicleTypeModel vehicleType)
    {
        _vehicleType = new()
        {
            Id = vehicleType.Id,
            Name = vehicleType.Name,
            Code = vehicleType.Code,
            Remarks = vehicleType.Remarks,
            Status = vehicleType.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteVehicleTypeId = id;
        _deleteVehicleTypeName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteVehicleTypeId = 0;
        _deleteVehicleTypeName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var vehicleType = _vehicleTypes.FirstOrDefault(l => l.Id == _deleteVehicleTypeId);
            if (vehicleType == null)
            {
                await _toastNotification.ShowAsync("Error", "Vehicle Type not found.", ToastType.Error);
                return;
            }

            vehicleType.Status = false;
            await VehicleData.InsertVehicleType(vehicleType);

            await _toastNotification.ShowAsync("Deleted", $"Vehicle Type '{vehicleType.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicleType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteVehicleTypeId = 0;
            _deleteVehicleTypeName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverVehicleTypeId = id;
        _recoverVehicleTypeName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverVehicleTypeId = 0;
        _recoverVehicleTypeName = string.Empty;
        await _recoverConfirmationDialog.HideAsync();
    }

    private async Task ToggleDeleted()
    {
        _showDeleted = !_showDeleted;
        await LoadData();
    }

    private async Task ConfirmRecover()
    {
        try
        {
            _isProcessing = true;
            await _recoverConfirmationDialog.HideAsync();

            var vehicleType = _vehicleTypes.FirstOrDefault(l => l.Id == _recoverVehicleTypeId);
            if (vehicleType == null)
            {
                await _toastNotification.ShowAsync("Error", "Vehicle Type not found.", ToastType.Error);
                return;
            }

            vehicleType.Status = true;
            await VehicleData.InsertVehicleType(vehicleType);

            await _toastNotification.ShowAsync("Recovered", $"Vehicle Type '{vehicleType.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicleType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverVehicleTypeId = 0;
            _recoverVehicleTypeName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _vehicleType.Name = _vehicleType.Name?.Trim() ?? "";
        _vehicleType.Name = _vehicleType.Name?.ToUpper() ?? "";

        _vehicleType.Remarks = _vehicleType.Remarks?.Trim() ?? "";
        _vehicleType.Status = true;


        if (_vehicleType.Id == 0)
            _vehicleType.Code = await GenerateCodes.GenerateVehicleTypeCode();

        if (string.IsNullOrWhiteSpace(_vehicleType.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle Type name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_vehicleType.Remarks))
            _vehicleType.Remarks = null;

        if (_vehicleType.Id > 0)
        {
            var existingVehicleType = _vehicleTypes.FirstOrDefault(_ => _.Id != _vehicleType.Id && _.Name.Equals(_vehicleType.Name, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleType is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle Type name '{_vehicleType.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingVehicleType = _vehicleTypes.FirstOrDefault(_ => _.Name.Equals(_vehicleType.Name, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleType is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle Type name '{_vehicleType.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveVehicleType()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();

            if (!await ValidateForm())
            {
                _isProcessing = false;
                return;
            }

            await _toastNotification.ShowAsync("Processing", "Please wait while the vehicle type is being saved...", ToastType.Info);

            await VehicleData.InsertVehicleType(_vehicleType);

            await _toastNotification.ShowAsync("Saved", $"Vehicle Type '{_vehicleType.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicleType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Vehicle Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }
    #endregion

    #region Exporting
    private async Task ExportExcel()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Please wait while the report is being exported...", ToastType.Info);

            var (stream, fileName) = await VehicleTypeExport.ExportMaster(_vehicleTypes, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Vehicle Type data exported to Excel successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while exporting to Excel: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ExportPdf()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Please wait while the report is being exported...", ToastType.Info);

            var (stream, fileName) = await VehicleTypeExport.ExportMaster(_vehicleTypes, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Vehicle Type data exported to PDF successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while exporting to PDF: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Utilities
    private async Task EditSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
            OnEditVehicleType(selectedRecords[0]);
    }

    private async Task DeleteSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
        {
            if (selectedRecords[0].Status)
                await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].Name);
            else
                await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].Name);
        }
    }

    private async Task ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.AdminVehicleType, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

    private void NavigateToDashboard() =>
        NavigationManager.NavigateTo(PageRouteNames.Dashboard);

    private async Task Logout() =>
        await AuthenticationService.Logout(DataStorageService, NavigationManager, VibrationService);

    public async ValueTask DisposeAsync()
    {
        if (_hotKeysContext is not null)
            await _hotKeysContext.DisposeAsync();

        GC.SuppressFinalize(this);
    }
    #endregion
}