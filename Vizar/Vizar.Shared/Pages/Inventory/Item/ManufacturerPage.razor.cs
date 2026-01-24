using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.Item;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Inventory.Item;

public partial class ManufacturerPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private ManufacturerModel _manufacturer = new();

    private List<ManufacturerModel> _manufacturers = [];

    private SfGrid<ManufacturerModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteManufacturerId = 0;
    private string _deleteManufacturerName = string.Empty;

    private int _recoverManufacturerId = 0;
    private string _recoverManufacturerName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveManufacturer, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _manufacturers = await CommonData.LoadTableData<ManufacturerModel>(TableNames.Manufacturer);

        if (!_showDeleted)
            _manufacturers = [.. _manufacturers.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditManufacturer(ManufacturerModel manufacturer)
    {
        _manufacturer = new()
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            Code = manufacturer.Code,
            Remarks = manufacturer.Remarks,
            Status = manufacturer.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteManufacturerId = id;
        _deleteManufacturerName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteManufacturerId = 0;
        _deleteManufacturerName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var manufacturer = _manufacturers.FirstOrDefault(l => l.Id == _deleteManufacturerId);
            if (manufacturer == null)
            {
                await _toastNotification.ShowAsync("Error", "Manufacturer not found.", ToastType.Error);
                return;
            }

            manufacturer.Status = false;
            await ItemData.InsertManufacturer(manufacturer);

            await _toastNotification.ShowAsync("Deleted", $"Manufacturer '{manufacturer.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminManufacturer, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Manufacturer: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteManufacturerId = 0;
            _deleteManufacturerName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverManufacturerId = id;
        _recoverManufacturerName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverManufacturerId = 0;
        _recoverManufacturerName = string.Empty;
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

            var manufacturer = _manufacturers.FirstOrDefault(l => l.Id == _recoverManufacturerId);
            if (manufacturer == null)
            {
                await _toastNotification.ShowAsync("Error", "Manufacturer not found.", ToastType.Error);
                return;
            }

            manufacturer.Status = true;
            await ItemData.InsertManufacturer(manufacturer);

            await _toastNotification.ShowAsync("Recovered", $"Manufacturer '{manufacturer.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminManufacturer, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Manufacturer: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverManufacturerId = 0;
            _recoverManufacturerName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _manufacturer.Name = _manufacturer.Name?.Trim() ?? "";
        _manufacturer.Name = _manufacturer.Name?.ToUpper() ?? "";

        _manufacturer.Remarks = _manufacturer.Remarks?.Trim() ?? "";
        _manufacturer.Status = true;


        if (_manufacturer.Id == 0)
            _manufacturer.Code = await GenerateCodes.GenerateManufacturerCode();

        if (string.IsNullOrWhiteSpace(_manufacturer.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Manufacturer name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_manufacturer.Remarks))
            _manufacturer.Remarks = null;

        if (_manufacturer.Id > 0)
        {
            var existingManufacturer = _manufacturers.FirstOrDefault(_ => _.Id != _manufacturer.Id && _.Name.Equals(_manufacturer.Name, StringComparison.OrdinalIgnoreCase));
            if (existingManufacturer is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Manufacturer name '{_manufacturer.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingManufacturer = _manufacturers.FirstOrDefault(_ => _.Name.Equals(_manufacturer.Name, StringComparison.OrdinalIgnoreCase));
            if (existingManufacturer is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Manufacturer name '{_manufacturer.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveManufacturer()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the manufacturer is being saved...", ToastType.Info);

            await ItemData.InsertManufacturer(_manufacturer);

            await _toastNotification.ShowAsync("Saved", $"Manufacturer '{_manufacturer.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminManufacturer, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Manufacturer: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await ManufacturerExport.ExportMaster(_manufacturers, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Manufacturer data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await ManufacturerExport.ExportMaster(_manufacturers, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Manufacturer data exported to PDF successfully.", ToastType.Success);
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
            OnEditManufacturer(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminManufacturer, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.InventoryDashboard);

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