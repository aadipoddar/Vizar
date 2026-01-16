using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Purchase;
using VizarLibrary.Data.Inventory.Stock;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Inventory.Stock;

public partial class ItemStockAdjustmentPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;

    private UserModel _user;

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private DateTime _transactionDateTime = DateTime.Now;
    private string _transactionNo = string.Empty;

    private FinancialYearModel _selectedFinancialYear = new();
    private ItemModel? _selectedItem = new();
    private ItemStockAdjustmentCartModel _selectedCart = new();

    private List<ItemModel> _items = [];
    private List<ItemStockAdjustmentCartModel> _cart = [];
    private List<ItemStockSummaryModel> _stockSummary = [];

    private SfAutoComplete<ItemModel?, ItemModel> _sfItemAutoComplete;
    private SfGrid<ItemStockAdjustmentCartModel> _sfCartGrid;

    private ToastNotification _toastNotification;

    #region Load Data
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, UserRoles.Inventory);
        await LoadData();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadData()
    {
        _hotKeysContext = HotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.Enter, AddItemToCart, "Add item to cart", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, () => _sfItemAutoComplete.FocusAsync(), "Focus on item input", Exclude.None)
            .Add(ModCode.Ctrl, Code.S, SaveTransaction, "Save the transaction", Exclude.None)
            .Add(ModCode.Ctrl, Code.H, NavigateToTransactionHistoryPage, "Open transaction history", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Go to dashboard", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(Code.Delete, RemoveSelectedCartItem, "Delete selected cart item", Exclude.None)
            .Add(Code.Insert, EditSelectedCartItem, "Edit selected cart item", Exclude.None);

        _transactionDateTime = await CommonData.LoadCurrentDateTime();
        _transactionNo = await GenerateCodes.GenerateItemStockAdjustmentTransactionNo(_transactionDateTime);
        await LoadStock();
        await LoadItems();
        await LoadExistingCart();
    }

    private async Task LoadStock()
    {
        try
        {
            _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_transactionDateTime);
            _stockSummary = await ItemStockData.LoadItemStockSummaryByDate(_transactionDateTime, _transactionDateTime);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Stock Data", ex.Message, ToastType.Error);
        }
    }

    private async Task LoadItems()
    {
        try
        {
            _items = await PurchaseData.LoadItemByPartyPurchaseDateTime(0, _transactionDateTime);

            _items = [.. _items.OrderBy(s => s.Name)];
            _items.Add(new()
            {
                Id = 0,
                Name = "Create New Item ..."
            });
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Items", ex.Message, ToastType.Error);
        }
    }

    private async Task LoadExistingCart()
    {
        try
        {
            _cart.Clear();

            if (await DataStorageService.LocalExists(StorageFileNames.ItemStockAdjustmentCartDataFileName))
                _cart = System.Text.Json.JsonSerializer.Deserialize<List<ItemStockAdjustmentCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.ItemStockAdjustmentCartDataFileName));
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Existing Cart", ex.Message, ToastType.Error);
            await DeleteLocalFiles();
        }
        finally
        {
            await SaveTransactionFile();
        }
    }
    #endregion

    #region Change Events
    private async Task OnTransactionDateChanged(Syncfusion.Blazor.Calendars.ChangedEventArgs<DateTime> args)
    {
        _transactionDateTime = args.Value;
        await LoadStock();
        await LoadItems();
        await SaveTransactionFile();
    }
    #endregion

    #region Cart
    private async Task OnItemChanged(ChangeEventArgs<ItemModel?, ItemModel> args)
    {
        if (args.Value is null)
            return;

        if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminItem, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminItem);

            return;
        }

        _selectedItem = args.Value;

        if (_selectedItem is null)
            _selectedCart = new()
            {
                ItemId = 0,
                ItemName = "",
                Stock = 0,
                Quantity = 1,
                Total = 0,
                Rate = 0,
            };

        else
        {
            _selectedCart.Stock = _stockSummary.FirstOrDefault(s => s.ItemId == _selectedItem.Id)?.ClosingStock ?? 0;
            _selectedCart.Quantity = _stockSummary.FirstOrDefault(s => s.ItemId == _selectedItem.Id)?.ClosingStock ?? 0;
            _selectedCart.Rate = _selectedItem.Rate;
            _selectedCart.Total = _selectedCart.Rate * _selectedCart.Quantity;
            _selectedCart.IdentificationNo = null;
        }

        UpdateSelectedItemFinancialDetails();
    }

    private void OnItemQuantityChanged(ChangeEventArgs<decimal> args)
    {
        _selectedCart.Quantity = args.Value;
        UpdateSelectedItemFinancialDetails();
    }

    private void UpdateSelectedItemFinancialDetails()
    {
        if (_selectedItem is null)
            return;

        _selectedCart.ItemId = _selectedItem.Id;
        _selectedCart.ItemName = _selectedItem.Name;
        _selectedCart.Rate = _selectedItem.Rate;
        _selectedCart.Stock = _stockSummary.FirstOrDefault(s => s.ItemId == _selectedItem.Id)?.ClosingStock ?? 0;
        _selectedCart.Total = _selectedCart.Quantity * _selectedCart.Rate;

        StateHasChanged();
    }

    private async Task AddItemToCart()
    {
        if (_selectedItem is null || _selectedItem.Id <= 0)
        {
            await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
            return;
        }

        UpdateSelectedItemFinancialDetails();

        var existingItem = _cart.FirstOrDefault(s => s.ItemId == _selectedCart.ItemId);
        if (existingItem is not null)
        {
            existingItem.Quantity = _selectedCart.Quantity;
            existingItem.Rate = _selectedCart.Rate;
        }
        else
            _cart.Add(new()
            {
                ItemId = _selectedCart.ItemId,
                ItemName = _selectedCart.ItemName,
                IdentificationNo = _selectedCart.IdentificationNo,
                Stock = _selectedCart.Stock,
                Quantity = _selectedCart.Quantity,
                Rate = _selectedCart.Rate,
                Total = _selectedCart.Total
            });

        _selectedItem = null;
        _selectedCart = new();

        await _sfItemAutoComplete.FocusAsync();
        await SaveTransactionFile();
    }

    private async Task EditSelectedCartItem()
    {
        if (_sfCartGrid is null || _sfCartGrid.SelectedRecords is null || _sfCartGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfCartGrid.SelectedRecords.First();
        await EditCartItem(selectedCartItem);
    }

    private async Task EditCartItem(ItemStockAdjustmentCartModel cartItem)
    {
        _selectedItem = _items.FirstOrDefault(s => s.Id == cartItem.ItemId);

        if (_selectedItem is null)
            return;

        _selectedCart = new()
        {
            ItemId = cartItem.ItemId,
            ItemName = cartItem.ItemName,
            IdentificationNo = cartItem.IdentificationNo,
            Stock = _stockSummary.FirstOrDefault(s => s.ItemId == cartItem.ItemId)?.ClosingStock ?? 0,
            Quantity = cartItem.Quantity,
            Rate = cartItem.Rate,
            Total = cartItem.Total
        };

        await _sfItemAutoComplete.FocusAsync();
        UpdateSelectedItemFinancialDetails();
        await RemoveItemFromCart(cartItem);
    }

    private async Task RemoveSelectedCartItem()
    {
        if (_sfCartGrid is null || _sfCartGrid.SelectedRecords is null || _sfCartGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfCartGrid.SelectedRecords.First();
        await RemoveItemFromCart(selectedCartItem);
    }

    private async Task RemoveItemFromCart(ItemStockAdjustmentCartModel cartItem)
    {
        _cart.Remove(cartItem);
        await SaveTransactionFile();
    }
    #endregion

    #region Saving
    private async Task UpdateFinancialDetails()
    {
        foreach (var item in _cart)
        {
            item.Stock = _stockSummary.FirstOrDefault(s => s.ItemId == item.ItemId)?.ClosingStock ?? 0;
            item.Quantity = item.Quantity;
            item.Total = item.Rate * item.Quantity;

            if (string.IsNullOrWhiteSpace(item.IdentificationNo))
                item.IdentificationNo = null;
        }

        #region Financial Year
        _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_transactionDateTime);
        if (_selectedFinancialYear is null || _selectedFinancialYear.Locked || !_selectedFinancialYear.Status)
        {
            await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
            _transactionDateTime = await CommonData.LoadCurrentDateTime();
            _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_transactionDateTime);
            _stockSummary = await ItemStockData.LoadItemStockSummaryByDate(_transactionDateTime, _transactionDateTime);
        }
        #endregion

        _transactionNo = await GenerateCodes.GenerateItemStockAdjustmentTransactionNo(_transactionDateTime);
    }

    private async Task SaveTransactionFile()
    {
        if (_isProcessing || _isLoading)
            return;

        try
        {
            _isProcessing = true;

            await UpdateFinancialDetails();

            await DataStorageService.LocalSaveAsync(StorageFileNames.ItemStockAdjustmentCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cart));
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction Data", ex.Message, ToastType.Error);
        }
        finally
        {
            if (_sfCartGrid is not null)
                await _sfCartGrid?.Refresh();

            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task<bool> ValidateForm()
    {
        if (_cart.Count == 0)
        {
            await _toastNotification.ShowAsync("Cart is Empty", "Please add at least one item to the cart before saving the transaction.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_transactionNo))
        {
            await _toastNotification.ShowAsync("Transaction Number Missing", "Transaction number is missing for the adjustment.", ToastType.Warning);
            return false;
        }

        if (_transactionDateTime == default)
        {
            await _toastNotification.ShowAsync("Transaction Date Missing", "Please select a valid transaction date for the adjustment.", ToastType.Warning);
            return false;
        }

        if (_selectedFinancialYear is null || _selectedFinancialYear.Id <= 0)
        {
            await _toastNotification.ShowAsync("Financial Year Not Found", "The transaction date does not fall within any financial year. Please check the date and try again.", ToastType.Error);
            return false;
        }

        if (_selectedFinancialYear.Locked)
        {
            await _toastNotification.ShowAsync("Financial Year Locked", "The financial year for the selected transaction date is locked. Please select a different date.", ToastType.Error);
            return false;
        }

        if (!_selectedFinancialYear.Status)
        {
            await _toastNotification.ShowAsync("Financial Year Inactive", "The financial year for the selected transaction date is inactive. Please select a different date.", ToastType.Error);
            return false;
        }

        return true;
    }

    private async Task SaveTransaction()
    {
        if (_isProcessing || _isLoading)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();

            await SaveTransactionFile();

            if (!await ValidateForm())
            {
                _isProcessing = false;
                return;
            }

            await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

            await ItemStockData.SaveItemStockAdjustment(_transactionDateTime, _cart, _user.Id);
            await ResetPage();

            await _toastNotification.ShowAsync("Save Transaction", "Transaction saved successfully!", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Saving Transaction", ex.Message, ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task DeleteLocalFiles() =>
        await DataStorageService.LocalRemove(StorageFileNames.ItemStockAdjustmentCartDataFileName);
    #endregion

    #region Utilities
    private async Task ResetPage()
    {
        await DeleteLocalFiles();
        NavigationManager.NavigateTo(PageRouteNames.ItemStockAdjustment, true);
    }

    private async Task NavigateToTransactionHistoryPage()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportItemStock, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportItemStock);
    }

    private void NavigateToDashboard() =>
        NavigationManager.NavigateTo(PageRouteNames.Dashboard);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.InventoryDashboard);

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