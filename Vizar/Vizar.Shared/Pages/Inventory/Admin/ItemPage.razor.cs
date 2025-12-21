using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.Masters;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory.Item;

namespace Vizar.Shared.Pages.Inventory.Admin;

public partial class ItemPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private ItemModel _item = new();

    private List<ItemModel> _items = [];
    private List<ItemTypeModel> _itemTypes = [];
    private List<ItemCategoryModel> _itemCategories = [];
    private List<ManufacturerModel> _manufacturers = [];
    private List<TaxModel> _taxes = [];

    private ItemTypeModel? _selectedItemType = null;
    private ItemCategoryModel? _selectedItemCategory = null;
    private ManufacturerModel? _selectedManufacturer = null;
    private TaxModel? _selectedTax = null;

    private SfGrid<ItemModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteItemId = 0;
    private string _deleteItemName = string.Empty;

    private int _recoverItemId = 0;
    private string _recoverItemName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveItem, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _items = await CommonData.LoadTableDataByStatus<ItemModel>(TableNames.Item);
        _itemTypes = await CommonData.LoadTableDataByStatus<ItemTypeModel>(TableNames.ItemType);
        _itemCategories = await CommonData.LoadTableDataByStatus<ItemCategoryModel>(TableNames.ItemCategory);
        _manufacturers = await CommonData.LoadTableDataByStatus<ManufacturerModel>(TableNames.Manufacturer);
        _taxes = await CommonData.LoadTableDataByStatus<TaxModel>(TableNames.Tax);

        if (!_showDeleted)
            _items = [.. _items.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditItem(ItemModel item)
    {
        _item = new()
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code,
            ItemTypeId = item.ItemTypeId,
            ItemCategoryId = item.ItemCategoryId,
            ManufacturerId = item.ManufacturerId,
            Rate = item.Rate,
            TaxId = item.TaxId,
            UnitOfMeasurement = item.UnitOfMeasurement,
            ReorderLevel = item.ReorderLevel,
            Remarks = item.Remarks,
            Status = item.Status
        };

        _selectedItemType = _itemTypes.FirstOrDefault(t => t.Id == item.ItemTypeId);
        _selectedItemCategory = _itemCategories.FirstOrDefault(c => c.Id == item.ItemCategoryId);
        _selectedManufacturer = _manufacturers.FirstOrDefault(m => m.Id == item.ManufacturerId);
        _selectedTax = _taxes.FirstOrDefault(t => t.Id == item.TaxId);

        StateHasChanged();
    }

    private void OnItemTypeChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ItemTypeModel, ItemTypeModel> args)
    {
        _selectedItemType = args.ItemData;
        _item.ItemTypeId = args.ItemData?.Id ?? 0;
    }

    private void OnItemCategoryChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ItemCategoryModel, ItemCategoryModel> args)
    {
        _selectedItemCategory = args.ItemData;
        _item.ItemCategoryId = args.ItemData?.Id ?? 0;
    }

    private void OnManufacturerChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ManufacturerModel, ManufacturerModel> args)
    {
        _selectedManufacturer = args.ItemData;
        _item.ManufacturerId = args.ItemData?.Id ?? 0;
    }

    private void OnTaxChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<TaxModel, TaxModel> args)
    {
        _selectedTax = args.ItemData;
        _item.TaxId = args.ItemData?.Id ?? 0;
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteItemId = id;
        _deleteItemName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteItemId = 0;
        _deleteItemName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var item = _items.FirstOrDefault(l => l.Id == _deleteItemId);
            if (item == null)
            {
                await _toastNotification.ShowAsync("Error", "Item not found.", ToastType.Error);
                return;
            }

            item.Status = false;
            await ItemData.InsertItem(item);

            await _toastNotification.ShowAsync("Deleted", $"Item '{item.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItem, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Item: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteItemId = 0;
            _deleteItemName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverItemId = id;
        _recoverItemName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverItemId = 0;
        _recoverItemName = string.Empty;
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

            var item = _items.FirstOrDefault(l => l.Id == _recoverItemId);
            if (item == null)
            {
                await _toastNotification.ShowAsync("Error", "Item not found.", ToastType.Error);
                return;
            }

            item.Status = true;
            await ItemData.InsertItem(item);

            await _toastNotification.ShowAsync("Recovered", $"Item '{item.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItem, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Item: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverItemId = 0;
            _recoverItemName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _item.Name = _item.Name?.Trim() ?? "";
        _item.Name = _item.Name?.ToUpper() ?? "";

        _item.UnitOfMeasurement = _item.UnitOfMeasurement?.Trim() ?? "";
        _item.UnitOfMeasurement = _item.UnitOfMeasurement?.ToUpper() ?? "";

        _item.Remarks = _item.Remarks?.Trim() ?? "";
        _item.Status = true;

        if (_item.Id == 0)
            _item.Code = await GenerateCodes.GenerateItemCode();

        if (string.IsNullOrWhiteSpace(_item.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Item name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (_item.ItemTypeId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Item Type is required. Please select an item type.", ToastType.Warning);
            return false;
        }

        if (_item.ItemCategoryId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Item Category is required. Please select an item category.", ToastType.Warning);
            return false;
        }

        if (_item.ManufacturerId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Manufacturer is required. Please select a manufacturer.", ToastType.Warning);
            return false;
        }

        if (_item.TaxId == 0)
        {
            await _toastNotification.ShowAsync("Validation", "Tax is required. Please select a tax.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_item.UnitOfMeasurement))
        {
            await _toastNotification.ShowAsync("Validation", "Unit of Measurement is required. Please enter a valid unit.", ToastType.Warning);
            return false;
        }

        if (_item.Rate <= 0)
        {
            await _toastNotification.ShowAsync("Validation", "Rate must be greater than 0. Please enter a valid rate.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_item.Remarks))
            _item.Remarks = null;

        if (_item.Id > 0)
        {
            var existingItem = _items.FirstOrDefault(_ => _.Id != _item.Id && _.Name.Equals(_item.Name, StringComparison.OrdinalIgnoreCase));
            if (existingItem is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Item name '{_item.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingItem = _items.FirstOrDefault(_ => _.Name.Equals(_item.Name, StringComparison.OrdinalIgnoreCase));
            if (existingItem is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Item name '{_item.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveItem()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the item is being saved...", ToastType.Info);

            await ItemData.InsertItem(_item);

            await _toastNotification.ShowAsync("Saved", $"Item '{_item.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItem, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Item: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await ItemExcelExport.ExportMaster(_items);
            await SaveAndViewService.SaveAndView(fileName, stream);
            await _toastNotification.ShowAsync("Exported", "Item data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await ItemPDFExport.ExportMaster(_items);
            await SaveAndViewService.SaveAndView(fileName, stream);
            await _toastNotification.ShowAsync("Exported", "Item data exported to PDF successfully.", ToastType.Success);
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
    private string GetItemTypeName(int itemTypeId) =>
        _itemTypes.FirstOrDefault(t => t.Id == itemTypeId)?.Name ?? "N/A";

    private string GetItemCategoryName(int itemCategoryId) =>
        _itemCategories.FirstOrDefault(c => c.Id == itemCategoryId)?.Name ?? "N/A";

    private string GetManufacturerName(int manufacturerId) =>
        _manufacturers.FirstOrDefault(m => m.Id == manufacturerId)?.Name ?? "N/A";

    private string GetTaxName(int taxId) =>
        _taxes.FirstOrDefault(t => t.Id == taxId)?.Name ?? "N/A";

    private async Task EditSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
            OnEditItem(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminItem, true);

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