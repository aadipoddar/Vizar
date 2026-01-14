using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.Masters;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory.Item;

namespace Vizar.Shared.Pages.Inventory.Admin;

public partial class ItemCategoryPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private ItemCategoryModel _itemCategory = new();

    private List<ItemCategoryModel> _itemCategories = [];

    private SfGrid<ItemCategoryModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteItemCategoryId = 0;
    private string _deleteItemCategoryName = string.Empty;

    private int _recoverItemCategoryId = 0;
    private string _recoverItemCategoryName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveItemCategory, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _itemCategories = await CommonData.LoadTableData<ItemCategoryModel>(TableNames.ItemCategory);

        if (!_showDeleted)
            _itemCategories = [.. _itemCategories.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditItemCategory(ItemCategoryModel itemCategory)
    {
        _itemCategory = new()
        {
            Id = itemCategory.Id,
            Name = itemCategory.Name,
            Code = itemCategory.Code,
            Remarks = itemCategory.Remarks,
            Status = itemCategory.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteItemCategoryId = id;
        _deleteItemCategoryName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteItemCategoryId = 0;
        _deleteItemCategoryName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var itemCategory = _itemCategories.FirstOrDefault(l => l.Id == _deleteItemCategoryId);
            if (itemCategory == null)
            {
                await _toastNotification.ShowAsync("Error", "Item Category not found.", ToastType.Error);
                return;
            }

            itemCategory.Status = false;
            await ItemData.InsertItemCategory(itemCategory);

            await _toastNotification.ShowAsync("Deleted", $"Item Category '{itemCategory.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItemCategory, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Item Category: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteItemCategoryId = 0;
            _deleteItemCategoryName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverItemCategoryId = id;
        _recoverItemCategoryName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverItemCategoryId = 0;
        _recoverItemCategoryName = string.Empty;
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

            var itemCategory = _itemCategories.FirstOrDefault(l => l.Id == _recoverItemCategoryId);
            if (itemCategory == null)
            {
                await _toastNotification.ShowAsync("Error", "Item Category not found.", ToastType.Error);
                return;
            }

            itemCategory.Status = true;
            await ItemData.InsertItemCategory(itemCategory);

            await _toastNotification.ShowAsync("Recovered", $"Item Category '{itemCategory.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItemCategory, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Item Category: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverItemCategoryId = 0;
            _recoverItemCategoryName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _itemCategory.Name = _itemCategory.Name?.Trim() ?? "";
        _itemCategory.Name = _itemCategory.Name?.ToUpper() ?? "";

        _itemCategory.Remarks = _itemCategory.Remarks?.Trim() ?? "";
        _itemCategory.Status = true;


        if (_itemCategory.Id == 0)
            _itemCategory.Code = await GenerateCodes.GenerateItemCategoryCode();

        if (string.IsNullOrWhiteSpace(_itemCategory.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Item Category name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_itemCategory.Remarks))
            _itemCategory.Remarks = null;

        if (_itemCategory.Id > 0)
        {
            var existingRawMaterialCategory = _itemCategories.FirstOrDefault(_ => _.Id != _itemCategory.Id && _.Name.Equals(_itemCategory.Name, StringComparison.OrdinalIgnoreCase));
            if (existingRawMaterialCategory is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Item Category name '{_itemCategory.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingRawMaterialCategory = _itemCategories.FirstOrDefault(_ => _.Name.Equals(_itemCategory.Name, StringComparison.OrdinalIgnoreCase));
            if (existingRawMaterialCategory is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Item Category name '{_itemCategory.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveItemCategory()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the category is being saved...", ToastType.Info);

            await ItemData.InsertItemCategory(_itemCategory);

            await _toastNotification.ShowAsync("Saved", $"Item Category '{_itemCategory.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItemCategory, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Item Category: {ex.Message}", ToastType.Error);
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
            await _toastNotification.ShowAsync("Exporting", "Exporting to Excel...", ToastType.Info);

            var (stream, fileName) = await ItemCategoryExcelExport.ExportMaster(_itemCategories);
            await SaveAndViewService.SaveAndView(fileName, stream);
            await _toastNotification.ShowAsync("Exported", "Item Category data exported to Excel successfully.", ToastType.Success);
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
            await _toastNotification.ShowAsync("Exporting", "Exporting to PDF...", ToastType.Info);

            var (stream, fileName) = await ItemCategoryPDFExport.ExportMaster(_itemCategories);
            await SaveAndViewService.SaveAndView(fileName, stream);
            await _toastNotification.ShowAsync("Exported", "Item Category data exported to PDF successfully.", ToastType.Success);
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
            OnEditItemCategory(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminVoucher, true);

    private async Task NavigateBack() =>
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