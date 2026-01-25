using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Masters;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Service;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Service;

public partial class GaragePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private GarageModel _garage = new();

    private List<GarageModel> _garages = [];

    private SfGrid<GarageModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteGarageId = 0;
    private string _deleteGarageName = string.Empty;

    private int _recoverGarageId = 0;
    private string _recoverGarageName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveGarage, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _garages = await CommonData.LoadTableData<GarageModel>(TableNames.Garage);

        if (!_showDeleted)
            _garages = [.. _garages.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditGarage(GarageModel garage)
    {
        _garage = new()
        {
            Id = garage.Id,
            Name = garage.Name,
            External = garage.External,
            Remarks = garage.Remarks,
            Status = garage.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteGarageId = id;
        _deleteGarageName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteGarageId = 0;
        _deleteGarageName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var garage = _garages.FirstOrDefault(l => l.Id == _deleteGarageId);
            if (garage == null)
            {
                await _toastNotification.ShowAsync("Error", "Garage not found.", ToastType.Error);
                return;
            }

            garage.Status = false;
            await GarageData.InsertGarage(garage);

            await _toastNotification.ShowAsync("Deleted", $"Garage '{garage.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminGarage, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Garage: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteGarageId = 0;
            _deleteGarageName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverGarageId = id;
        _recoverGarageName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverGarageId = 0;
        _recoverGarageName = string.Empty;
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

            var garage = _garages.FirstOrDefault(l => l.Id == _recoverGarageId);
            if (garage == null)
            {
                await _toastNotification.ShowAsync("Error", "Garage not found.", ToastType.Error);
                return;
            }

            garage.Status = true;
            await GarageData.InsertGarage(garage);

            await _toastNotification.ShowAsync("Recovered", $"Garage '{garage.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminGarage, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Garage: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverGarageId = 0;
            _recoverGarageName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _garage.Name = _garage.Name?.Trim() ?? "";
        _garage.Name = _garage.Name?.ToUpper() ?? "";

        _garage.Remarks = _garage.Remarks?.Trim() ?? "";
        _garage.Status = true;

        if (string.IsNullOrWhiteSpace(_garage.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Garage name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_garage.Remarks))
            _garage.Remarks = null;

        if (_garage.Id > 0)
        {
            var existingGarage = _garages.FirstOrDefault(_ => _.Id != _garage.Id && _.Name.Equals(_garage.Name, StringComparison.OrdinalIgnoreCase));
            if (existingGarage is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Garage name '{_garage.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingGarage = _garages.FirstOrDefault(_ => _.Name.Equals(_garage.Name, StringComparison.OrdinalIgnoreCase));
            if (existingGarage is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Garage name '{_garage.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveGarage()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the garage is being saved...", ToastType.Info);

            await GarageData.InsertGarage(_garage);

            await _toastNotification.ShowAsync("Saved", $"Garage '{_garage.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminGarage, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Garage: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await GarageExport.ExportMaster(_garages, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Garage data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await GarageExport.ExportMaster(_garages, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Garage data exported to PDF successfully.", ToastType.Success);
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
            OnEditGarage(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminGarage, true);

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
