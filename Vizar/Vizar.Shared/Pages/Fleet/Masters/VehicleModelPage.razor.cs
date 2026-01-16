using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Vehicle;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Masters;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Masters;

public partial class VehicleModelPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private VehicleModelModel _vehicleModel = new();

    private List<VehicleModelModel> _vehicleModels = [];
    private List<ManufacturerModel> _manufacturers = [];

    private ManufacturerModel? _selectedManufacturer = null;

    private SfGrid<VehicleModelModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteVehicleModelId = 0;
    private string _deleteVehicleModelName = string.Empty;

    private int _recoverVehicleModelId = 0;
    private string _recoverVehicleModelName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveVehicleModel, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _vehicleModels = await CommonData.LoadTableData<VehicleModelModel>(TableNames.VehicleModel);
        _manufacturers = await CommonData.LoadTableDataByStatus<ManufacturerModel>(TableNames.Manufacturer);

        if (!_showDeleted)
            _vehicleModels = [.. _vehicleModels.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditVehicleModel(VehicleModelModel vehicleModel)
    {
        _vehicleModel = new()
        {
            Id = vehicleModel.Id,
            Name = vehicleModel.Name,
            Code = vehicleModel.Code,
            ManufacturerId = vehicleModel.ManufacturerId,
            Remarks = vehicleModel.Remarks,
            Status = vehicleModel.Status
        };

        _selectedManufacturer = _manufacturers.FirstOrDefault(m => m.Id == vehicleModel.ManufacturerId);

        StateHasChanged();
    }

    private void OnManufacturerChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ManufacturerModel, ManufacturerModel> args)
    {
        _selectedManufacturer = args.ItemData;
        _vehicleModel.ManufacturerId = args.ItemData?.Id ?? 0;
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteVehicleModelId = id;
        _deleteVehicleModelName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteVehicleModelId = 0;
        _deleteVehicleModelName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var vehicleModel = _vehicleModels.FirstOrDefault(l => l.Id == _deleteVehicleModelId);
            if (vehicleModel == null)
            {
                await _toastNotification.ShowAsync("Error", "Vehicle Model not found.", ToastType.Error);
                return;
            }

            vehicleModel.Status = false;
            await VehicleData.InsertVehicleModel(vehicleModel);

            await _toastNotification.ShowAsync("Deleted", $"Vehicle Model '{vehicleModel.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicleModel, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle Model: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteVehicleModelId = 0;
            _deleteVehicleModelName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverVehicleModelId = id;
        _recoverVehicleModelName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverVehicleModelId = 0;
        _recoverVehicleModelName = string.Empty;
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

            var vehicleModel = _vehicleModels.FirstOrDefault(l => l.Id == _recoverVehicleModelId);
            if (vehicleModel == null)
            {
                await _toastNotification.ShowAsync("Error", "Vehicle Model not found.", ToastType.Error);
                return;
            }

            vehicleModel.Status = true;
            await VehicleData.InsertVehicleModel(vehicleModel);

            await _toastNotification.ShowAsync("Recovered", $"Vehicle Model '{vehicleModel.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicleModel, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle Model: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverVehicleModelId = 0;
            _recoverVehicleModelName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _vehicleModel.Name = _vehicleModel.Name?.Trim() ?? "";
        _vehicleModel.Name = _vehicleModel.Name?.ToUpper() ?? "";

        _vehicleModel.Code = _vehicleModel.Code?.Trim() ?? "";
        _vehicleModel.Code = _vehicleModel.Code?.ToUpper() ?? "";

        _vehicleModel.Remarks = _vehicleModel.Remarks?.Trim() ?? "";
        _vehicleModel.Status = true;

        if (string.IsNullOrWhiteSpace(_vehicleModel.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle Model name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_vehicleModel.Code))
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle Model code is required. Please enter a valid code.", ToastType.Warning);
            return false;
        }

        if (_vehicleModel.ManufacturerId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Manufacturer is required. Please select a manufacturer.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_vehicleModel.Remarks))
            _vehicleModel.Remarks = null;

        if (_vehicleModel.Id > 0)
        {
            var existingVehicleModelByName = _vehicleModels.FirstOrDefault(_ => _.Id != _vehicleModel.Id && _.Name.Equals(_vehicleModel.Name, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleModelByName is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle Model name '{_vehicleModel.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }

            var existingVehicleModelByCode = _vehicleModels.FirstOrDefault(_ => _.Id != _vehicleModel.Id && _.Code.Equals(_vehicleModel.Code, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleModelByCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle Model code '{_vehicleModel.Code}' already exists. Please choose a different code.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingVehicleModelByName = _vehicleModels.FirstOrDefault(_ => _.Name.Equals(_vehicleModel.Name, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleModelByName is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle Model name '{_vehicleModel.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }

            var existingVehicleModelByCode = _vehicleModels.FirstOrDefault(_ => _.Code.Equals(_vehicleModel.Code, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleModelByCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle Model code '{_vehicleModel.Code}' already exists. Please choose a different code.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveVehicleModel()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the vehicle model is being saved...", ToastType.Info);

            await VehicleData.InsertVehicleModel(_vehicleModel);

            await _toastNotification.ShowAsync("Saved", $"Vehicle Model '{_vehicleModel.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicleModel, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Vehicle Model: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await VehicleModelExport.ExportMaster(_vehicleModels, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Vehicle Model data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await VehicleModelExport.ExportMaster(_vehicleModels, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Vehicle Model data exported to PDF successfully.", ToastType.Success);
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
            OnEditVehicleModel(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminVehicleModel, true);

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
