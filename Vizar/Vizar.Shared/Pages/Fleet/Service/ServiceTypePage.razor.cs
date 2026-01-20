using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Service;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Service;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Service;

public partial class ServiceTypePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private ServiceTypeModel _serviceType = new();

    private List<ServiceTypeModel> _serviceTypes = [];

    private SfGrid<ServiceTypeModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteServiceTypeId = 0;
    private string _deleteServiceTypeName = string.Empty;

    private int _recoverServiceTypeId = 0;
    private string _recoverServiceTypeName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveServiceType, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _serviceTypes = await CommonData.LoadTableData<ServiceTypeModel>(TableNames.ServiceType);

        if (!_showDeleted)
            _serviceTypes = [.. _serviceTypes.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditServiceType(ServiceTypeModel serviceType)
    {
        _serviceType = new()
        {
            Id = serviceType.Id,
            Name = serviceType.Name,
            Code = serviceType.Code,
            Rate = serviceType.Rate,
            Remarks = serviceType.Remarks,
            Status = serviceType.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteServiceTypeId = id;
        _deleteServiceTypeName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteServiceTypeId = 0;
        _deleteServiceTypeName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var serviceType = _serviceTypes.FirstOrDefault(l => l.Id == _deleteServiceTypeId);
            if (serviceType == null)
            {
                await _toastNotification.ShowAsync("Error", "Service Type not found.", ToastType.Error);
                return;
            }

            serviceType.Status = false;
            await ServiceData.InsertServiceType(serviceType);

            await _toastNotification.ShowAsync("Deleted", $"Service Type '{serviceType.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminServiceType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Service Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteServiceTypeId = 0;
            _deleteServiceTypeName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverServiceTypeId = id;
        _recoverServiceTypeName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverServiceTypeId = 0;
        _recoverServiceTypeName = string.Empty;
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

            var serviceType = _serviceTypes.FirstOrDefault(l => l.Id == _recoverServiceTypeId);
            if (serviceType == null)
            {
                await _toastNotification.ShowAsync("Error", "Service Type not found.", ToastType.Error);
                return;
            }

            serviceType.Status = true;
            await ServiceData.InsertServiceType(serviceType);

            await _toastNotification.ShowAsync("Recovered", $"Service Type '{serviceType.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminServiceType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Service Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverServiceTypeId = 0;
            _recoverServiceTypeName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _serviceType.Name = _serviceType.Name?.Trim() ?? "";
        _serviceType.Name = _serviceType.Name?.ToUpper() ?? "";

        _serviceType.Remarks = _serviceType.Remarks?.Trim() ?? "";
        _serviceType.Status = true;


        if (_serviceType.Id == 0)
            _serviceType.Code = await GenerateCodes.GenerateServiceTypeCode();

        if (string.IsNullOrWhiteSpace(_serviceType.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Service Type name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (_serviceType.Rate < 0)
        {
            await _toastNotification.ShowAsync("Validation", "Rate cannot be negative. Please enter a valid rate.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_serviceType.Remarks))
            _serviceType.Remarks = null;

        if (_serviceType.Id > 0)
        {
            var existingServiceType = _serviceTypes.FirstOrDefault(_ => _.Id != _serviceType.Id && _.Name.Equals(_serviceType.Name, StringComparison.OrdinalIgnoreCase));
            if (existingServiceType is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Service Type name '{_serviceType.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingServiceType = _serviceTypes.FirstOrDefault(_ => _.Name.Equals(_serviceType.Name, StringComparison.OrdinalIgnoreCase));
            if (existingServiceType is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Service Type name '{_serviceType.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveServiceType()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the service type is being saved...", ToastType.Info);

            await ServiceData.InsertServiceType(_serviceType);

            await _toastNotification.ShowAsync("Saved", $"Service Type '{_serviceType.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminServiceType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Service Type: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await ServiceTypeExport.ExportMaster(_serviceTypes, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Service Type data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await ServiceTypeExport.ExportMaster(_serviceTypes, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Service Type data exported to PDF successfully.", ToastType.Success);
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
            OnEditServiceType(selectedRecords[0]);
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

    private void ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.AdminServiceType, true);

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
