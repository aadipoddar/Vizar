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

public partial class TaxPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private TaxModel _tax = new();

    private List<TaxModel> _taxes = [];

    private SfGrid<TaxModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteTaxId = 0;
    private string _deleteTaxName = string.Empty;

    private int _recoverTaxId = 0;
    private string _recoverTaxName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveTax, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _taxes = await CommonData.LoadTableData<TaxModel>(TableNames.Tax);

        if (!_showDeleted)
            _taxes = [.. _taxes.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditTax(TaxModel tax)
    {
        _tax = new()
        {
            Id = tax.Id,
            Name = tax.Name,
            Code = tax.Code,
            CGST = tax.CGST,
            SGST = tax.SGST,
            IGST = tax.IGST,
            Inclusive = tax.Inclusive,
            Extra = tax.Extra,
            Remarks = tax.Remarks,
            Status = tax.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteTaxId = id;
        _deleteTaxName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteTaxId = 0;
        _deleteTaxName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var tax = _taxes.FirstOrDefault(l => l.Id == _deleteTaxId);
            if (tax == null)
            {
                await _toastNotification.ShowAsync("Error", "Tax not found.", ToastType.Error);
                return;
            }

            tax.Status = false;
            await ItemData.InsertTax(tax);

            await _toastNotification.ShowAsync("Deleted", $"Tax '{tax.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminTax, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Tax: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteTaxId = 0;
            _deleteTaxName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverTaxId = id;
        _recoverTaxName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverTaxId = 0;
        _recoverTaxName = string.Empty;
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

            var tax = _taxes.FirstOrDefault(l => l.Id == _recoverTaxId);
            if (tax == null)
            {
                await _toastNotification.ShowAsync("Error", "Tax not found.", ToastType.Error);
                return;
            }

            tax.Status = true;
            await ItemData.InsertTax(tax);

            await _toastNotification.ShowAsync("Recovered", $"Tax '{tax.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminTax, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Tax: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverTaxId = 0;
            _recoverTaxName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _tax.Name = _tax.Name?.Trim() ?? "";
        _tax.Name = _tax.Name?.ToUpper() ?? "";

        _tax.Code = _tax.Code?.Trim() ?? "";
        _tax.Code = _tax.Code?.ToUpper() ?? "";

        _tax.Remarks = _tax.Remarks?.Trim() ?? "";
        _tax.Status = true;

        if (string.IsNullOrWhiteSpace(_tax.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Tax name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_tax.Code))
        {
            await _toastNotification.ShowAsync("Validation", "Tax code is required. Please enter a valid code.", ToastType.Warning);
            return false;
        }

        if (_tax.Inclusive && _tax.Extra)
        {
            await _toastNotification.ShowAsync("Validation", "Tax cannot be both Inclusive and Extra. Please select only one option.", ToastType.Warning);
            return false;
        }

        if (!_tax.Inclusive && !_tax.Extra)
        {
            await _toastNotification.ShowAsync("Validation", "Tax must be either Inclusive or Extra. Please select one option.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_tax.Remarks))
            _tax.Remarks = null;

        if (_tax.Id > 0)
        {
            var existingTax = _taxes.FirstOrDefault(_ => _.Id != _tax.Id && _.Name.Equals(_tax.Name, StringComparison.OrdinalIgnoreCase));
            if (existingTax is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Tax name '{_tax.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }

            var existingCode = _taxes.FirstOrDefault(_ => _.Id != _tax.Id && _.Code.Equals(_tax.Code, StringComparison.OrdinalIgnoreCase));
            if (existingCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Tax code '{_tax.Code}' already exists. Please choose a different code.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingTax = _taxes.FirstOrDefault(_ => _.Name.Equals(_tax.Name, StringComparison.OrdinalIgnoreCase));
            if (existingTax is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Tax name '{_tax.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }

            var existingCode = _taxes.FirstOrDefault(_ => _.Code.Equals(_tax.Code, StringComparison.OrdinalIgnoreCase));
            if (existingCode is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Tax code '{_tax.Code}' already exists. Please choose a different code.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveTax()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the tax is being saved...", ToastType.Info);

            await ItemData.InsertTax(_tax);

            await _toastNotification.ShowAsync("Saved", $"Tax '{_tax.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminTax, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Tax: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await TaxExport.ExportMaster(_taxes, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Tax data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await TaxExport.ExportMaster(_taxes, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Tax data exported to PDF successfully.", ToastType.Success);
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
            OnEditTax(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminTax, true);

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