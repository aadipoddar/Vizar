using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Masters;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Vehicle;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Vehicle;

public partial class VehiclePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private VehicleModel _vehicle = new() { PurchaseDate = DateTime.Now };

    private List<VehicleModel> _vehicles = [];
    private List<VehicleTypeModel> _vehicleTypes = [];
    private List<VehicleModelModel> _vehicleModels = [];

    private VehicleTypeModel? _selectedVehicleType = null;
    private VehicleModelModel? _selectedVehicleModel = null;

    private SfGrid<VehicleModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteVehicleId = 0;
    private string _deleteVehicleCode = string.Empty;

    private int _recoverVehicleId = 0;
    private string _recoverVehicleCode = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveVehicle, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _vehicles = await CommonData.LoadTableData<VehicleModel>(TableNames.Vehicle);
        _vehicleTypes = await CommonData.LoadTableDataByStatus<VehicleTypeModel>(TableNames.VehicleType);
        _vehicleModels = await CommonData.LoadTableDataByStatus<VehicleModelModel>(TableNames.VehicleModel);

        if (!_showDeleted)
            _vehicles = [.. _vehicles.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditVehicle(VehicleModel vehicle)
    {
        _vehicle = new()
        {
            Id = vehicle.Id,
            Code = vehicle.Code,
            ShortCode = vehicle.ShortCode,
            ChasisCode = vehicle.ChasisCode,
            VehicleTypeId = vehicle.VehicleTypeId,
            VehicleModelId = vehicle.VehicleModelId,
            PurchaseDate = vehicle.PurchaseDate,
            OpeningHour = vehicle.OpeningHour,
            OpeningKM = vehicle.OpeningKM,
            Remarks = vehicle.Remarks,
            Status = vehicle.Status
        };

        _selectedVehicleType = _vehicleTypes.FirstOrDefault(vt => vt.Id == vehicle.VehicleTypeId);
        _selectedVehicleModel = _vehicleModels.FirstOrDefault(vm => vm.Id == vehicle.VehicleModelId);

        StateHasChanged();
    }

    private void OnVehicleTypeChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<VehicleTypeModel, VehicleTypeModel> args)
    {
        _selectedVehicleType = args.ItemData;
        _vehicle.VehicleTypeId = args.ItemData?.Id ?? 0;
    }

    private void OnVehicleModelChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<VehicleModelModel, VehicleModelModel> args)
    {
        _selectedVehicleModel = args.ItemData;
        _vehicle.VehicleModelId = args.ItemData?.Id ?? 0;
    }

    private async Task ShowDeleteConfirmation(int id, string code)
    {
        _deleteVehicleId = id;
        _deleteVehicleCode = code;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteVehicleId = 0;
        _deleteVehicleCode = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var vehicle = _vehicles.FirstOrDefault(l => l.Id == _deleteVehicleId);
            if (vehicle == null)
            {
                await _toastNotification.ShowAsync("Error", "Vehicle not found.", ToastType.Error);
                return;
            }

            vehicle.Status = false;
            await VehicleData.InsertVehicle(vehicle);

            await _toastNotification.ShowAsync("Deleted", $"Vehicle '{vehicle.Code}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicle, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Vehicle: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteVehicleId = 0;
            _deleteVehicleCode = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string code)
    {
        _recoverVehicleId = id;
        _recoverVehicleCode = code;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverVehicleId = 0;
        _recoverVehicleCode = string.Empty;
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

            var vehicle = _vehicles.FirstOrDefault(l => l.Id == _recoverVehicleId);
            if (vehicle == null)
            {
                await _toastNotification.ShowAsync("Error", "Vehicle not found.", ToastType.Error);
                return;
            }

            vehicle.Status = true;
            await VehicleData.InsertVehicle(vehicle);

            await _toastNotification.ShowAsync("Recovered", $"Vehicle '{vehicle.Code}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicle, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Vehicle: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverVehicleId = 0;
            _recoverVehicleCode = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _vehicle.Code = _vehicle.Code?.Trim() ?? "";
        _vehicle.Code = _vehicle.Code?.ToUpper() ?? "";

        _vehicle.ShortCode = _vehicle.ShortCode?.Trim() ?? "";
        _vehicle.ShortCode = _vehicle.ShortCode?.ToUpper() ?? "";

        _vehicle.ChasisCode = _vehicle.ChasisCode?.Trim() ?? "";
        _vehicle.ChasisCode = _vehicle.ChasisCode?.ToUpper() ?? "";

        _vehicle.Remarks = _vehicle.Remarks?.Trim() ?? "";
        _vehicle.Status = true;

        if (string.IsNullOrWhiteSpace(_vehicle.Code))
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle code is required. Please enter a valid code.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_vehicle.ShortCode))
        {
            await _toastNotification.ShowAsync("Validation", "Short code is required. Please enter a valid short code.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_vehicle.ChasisCode))
        {
            await _toastNotification.ShowAsync("Validation", "Chasis code is required. Please enter a valid chasis code.", ToastType.Warning);
            return false;
        }

        if (_vehicle.VehicleTypeId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle type is required. Please select a vehicle type.", ToastType.Warning);
            return false;
        }

        if (_vehicle.VehicleModelId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle model is required. Please select a vehicle model.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_vehicle.Remarks))
            _vehicle.Remarks = null;

        if (_vehicle.Id > 0)
        {
            var existingVehicleByCode = _vehicles.FirstOrDefault(_ => _.Id != _vehicle.Id && _.Code.Equals(_vehicle.Code, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleByCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle code '{_vehicle.Code}' already exists. Please choose a different code.", ToastType.Warning);
                return false;
            }

            var existingVehicleByShortCode = _vehicles.FirstOrDefault(_ => _.Id != _vehicle.Id && _.ShortCode.Equals(_vehicle.ShortCode, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleByShortCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Short code '{_vehicle.ShortCode}' already exists. Please choose a different short code.", ToastType.Warning);
                return false;
            }

            var existingVehicleByChasisCode = _vehicles.FirstOrDefault(_ => _.Id != _vehicle.Id && _.ChasisCode.Equals(_vehicle.ChasisCode, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleByChasisCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Chasis code '{_vehicle.ChasisCode}' already exists. Please choose a different chasis code.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingVehicleByCode = _vehicles.FirstOrDefault(_ => _.Code.Equals(_vehicle.Code, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleByCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Vehicle code '{_vehicle.Code}' already exists. Please choose a different code.", ToastType.Warning);
                return false;
            }

            var existingVehicleByShortCode = _vehicles.FirstOrDefault(_ => _.ShortCode.Equals(_vehicle.ShortCode, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleByShortCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Short code '{_vehicle.ShortCode}' already exists. Please choose a different short code.", ToastType.Warning);
                return false;
            }

            var existingVehicleByChasisCode = _vehicles.FirstOrDefault(_ => _.ChasisCode.Equals(_vehicle.ChasisCode, StringComparison.OrdinalIgnoreCase));
            if (existingVehicleByChasisCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Chasis code '{_vehicle.ChasisCode}' already exists. Please choose a different chasis code.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveVehicle()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the vehicle is being saved...", ToastType.Info);

            await VehicleData.InsertVehicle(_vehicle);

            await _toastNotification.ShowAsync("Saved", $"Vehicle '{_vehicle.Code}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminVehicle, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Vehicle: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await VehicleExport.ExportMaster(_vehicles, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Vehicle data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await VehicleExport.ExportMaster(_vehicles, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Vehicle data exported to PDF successfully.", ToastType.Success);
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
            OnEditVehicle(selectedRecords[0]);
    }

    private async Task DeleteSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
        {
            if (selectedRecords[0].Status)
                await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].Code);
            else
                await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].Code);
        }
    }

    private async Task ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.AdminVehicle, true);

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
