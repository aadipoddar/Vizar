using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data;
using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.Data.Inventory.ItemIssue;
using VizarLibrary.Data.Inventory.Purchase;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.ItemIssue;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace Vizar.Shared.Pages.Inventory.ItemIssue;

public partial class ItemIssuePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;

    [Parameter] public int? Id { get; set; }

    private UserModel _user;

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private decimal _itemAfterTaxTotal = 0;

    private CompanyModel _selectedCompany = new();
    private GarageModel? _selectedGarage = new();
    private FinancialYearModel _selectedFinancialYear = new();
    private ItemModel? _selectedItem = new();
    private ItemIssueItemCartModel _selectedCart = new();
    private ItemIssueModel _itemIssue = new();

    private List<ItemStockSummaryModel> _stockSummary = [];
    private List<CompanyModel> _companies = [];
    private List<GarageModel> _garages = [];
    private List<ItemModel> _items = [];
    private List<VehicleModel> _vehicles = [];
    private List<ItemIssueItemCartModel> _cart = [];

    private SfAutoComplete<ItemModel?, ItemModel> _sfItemAutoComplete;
    private SfGrid<ItemIssueItemCartModel> _sfCartGrid;

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
            .Add(ModCode.Alt, Code.P, DownloadPdfInvoice, "Download PDF invoice", Exclude.None)
            .Add(ModCode.Alt, Code.E, DownloadExcelInvoice, "Download Excel invoice", Exclude.None)
            .Add(ModCode.Ctrl, Code.H, NavigateToTransactionHistoryPage, "Open transaction history", Exclude.None)
            .Add(ModCode.Ctrl, Code.I, NavigateToItemReport, "Open item report", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Go to dashboard", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(Code.Delete, RemoveSelectedCartItem, "Delete selected cart item", Exclude.None)
            .Add(Code.Insert, EditSelectedCartItem, "Edit selected cart item", Exclude.None);

        await LoadCompanies();
        await LoadGarages();
        await LoadExistingTransaction();
        await LoadItems();
        await LoadVehicles();
        await LoadExistingCart();
        await SaveTransactionFile();
    }

    private async Task LoadCompanies()
    {
        try
        {
            _companies = await CommonData.LoadTableDataByStatus<CompanyModel>(TableNames.Company);
            _companies = [.. _companies.OrderBy(s => s.Name)];
            _companies.Add(new()
            {
                Id = 0,
                Name = "Create New Company ..."
            });

            var mainCompanyId = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
            _selectedCompany = _companies.FirstOrDefault(s => s.Id.ToString() == mainCompanyId.Value) ?? throw new Exception("Main Company Not Found");
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Companies", ex.Message, ToastType.Error);
        }
    }

    private async Task LoadGarages()
    {
        try
        {
            _garages = await CommonData.LoadTableDataByStatus<GarageModel>(TableNames.Garage);
            _garages = [.. _garages.OrderBy(s => s.Name)];
            _garages.Add(new()
            {
                Id = 0,
                Name = "Create New Garage ..."
            });
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Garages", ex.Message, ToastType.Error);
        }
    }

    private async Task LoadExistingTransaction()
    {
        try
        {
            if (Id.HasValue)
            {
                _itemIssue = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, Id.Value);
                if (_itemIssue is null)
                {
                    await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
                    NavigationManager.NavigateTo(PageRouteNames.ItemIssue, true);
                }
            }

            else if (await DataStorageService.LocalExists(StorageFileNames.ItemIssueDataFileName))
                _itemIssue = System.Text.Json.JsonSerializer.Deserialize<ItemIssueModel>(await DataStorageService.LocalGetAsync(StorageFileNames.ItemIssueDataFileName));

            else
            {
                _itemIssue = new()
                {
                    Id = 0,
                    TransactionNo = string.Empty,
                    CompanyId = _selectedCompany.Id,
                    GarageId = null,
                    TransactionDateTime = await CommonData.LoadCurrentDateTime(),
                    FinancialYearId = (await FinancialYearData.LoadFinancialYearByDateTime(await CommonData.LoadCurrentDateTime())).Id,
                    CreatedBy = _user.Id,
                    TotalItems = 0,
                    TotalQuantity = 0,
                    TotalAmount = 0,
                    Remarks = "",
                    CreatedAt = DateTime.Now,
                    CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform(),
                    Status = true,
                    LastModifiedAt = null,
                    LastModifiedBy = null,
                    LastModifiedFromPlatform = null
                };
                await DeleteLocalFiles();
            }

            if (_itemIssue.CompanyId > 0)
                _selectedCompany = _companies.FirstOrDefault(s => s.Id == _itemIssue.CompanyId);
            else
            {
                _selectedCompany = _companies.FirstOrDefault();
                _itemIssue.CompanyId = _selectedCompany.Id;
            }

            if (_itemIssue.GarageId > 0)
                _selectedGarage = _garages.FirstOrDefault(s => s.Id == _itemIssue.GarageId);
            else
            {
                _selectedGarage = null;
                _itemIssue.GarageId = null;
            }

            _selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _itemIssue.FinancialYearId);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Transaction Data", ex.Message, ToastType.Error);
            await DeleteLocalFiles();
        }
        finally
        {
            await SaveTransactionFile();
        }
    }

    private async Task LoadItems()
    {
        try
        {
            _items = await PurchaseData.LoadItemByPartyPurchaseDateTime(0, _itemIssue.TransactionDateTime);
            _items = [.. _items.OrderBy(s => s.Name)];
            _items.Add(new()
            {
                Id = 0,
                Name = "Create New Item ..."
            });

            _stockSummary = await ItemStockData.LoadItemStockSummaryByDate(_itemIssue.TransactionDateTime, _itemIssue.TransactionDateTime);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Items", ex.Message, ToastType.Error);
        }
    }

    private async Task LoadVehicles()
    {
        try
        {
            _vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(TableNames.Vehicle);
            _vehicles = [.. _vehicles.OrderBy(s => s.Code)];
            _vehicles.Add(new()
            {
                Id = 0,
                Code = "Create New Vehicle ...",
                ShortCode = "New"
            });
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Vehicles", ex.Message, ToastType.Error);
        }
    }

    private async Task LoadExistingCart()
    {
        try
        {
            _cart.Clear();

            if (_itemIssue.Id > 0)
            {
                var existingCart = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, _itemIssue.Id);

                foreach (var item in existingCart)
                {
                    if (_items.FirstOrDefault(s => s.Id == item.ItemId) is null)
                    {
                        var rawMaterial = await CommonData.LoadTableDataById<ItemModel>(TableNames.Item, item.ItemId);
                        await _toastNotification.ShowAsync("Item Not Found", $"The item {rawMaterial?.Name} (ID: {item.ItemId}) in the existing transaction cart was not found in the available items list. It may have been deleted or is inaccessible.", ToastType.Error);
                        continue;
                    }

                    if (item.VehicleId is not null && _vehicles.FirstOrDefault(s => s.Id == item.VehicleId) is null)
                    {
                        var vehicle = await CommonData.LoadTableDataById<VehicleModel>(TableNames.Vehicle, item.VehicleId.Value);
                        await _toastNotification.ShowAsync("Vehicle Not Found", $"The vehicle {vehicle?.Code} (ID: {item.VehicleId}) associated with the item {_items.FirstOrDefault(s => s.Id == item.ItemId)?.Name ?? "Unknown Item"} in the existing transaction cart was not found in the available vehicles list. It may have been deleted or is inaccessible.", ToastType.Error);
                        continue;
                    }

                    _cart.Add(new()
                    {
                        ItemId = item.ItemId,
                        ItemName = _items.FirstOrDefault(s => s.Id == item.ItemId)?.Name ?? "",
                        VehicleId = item.VehicleId,
                        VehicleShortCode = item.VehicleId is not null ? _vehicles.FirstOrDefault(s => s.Id == item.VehicleId)?.ShortCode ?? null : null,
                        VehicleCode = item.VehicleId is not null ? _vehicles.FirstOrDefault(s => s.Id == item.VehicleId)?.Code ?? null : null,
                        CurrentHour = item.VehicleId is not null ? item.CurrentHour : null,
                        CurrentKM = item.VehicleId is not null ? item.CurrentKM : null,
                        IdentificationNo = item.IdentificationNo,
                        UnitOfMeasurement = item.UnitOfMeasurement,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        Total = item.Total,
                        Remarks = item.Remarks
                    });
                }
            }

            else if (await DataStorageService.LocalExists(StorageFileNames.ItemIssueCartDataFileName))
                _cart = System.Text.Json.JsonSerializer.Deserialize<List<ItemIssueItemCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.ItemIssueCartDataFileName));
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
    private async Task OnCompanyChanged(ChangeEventArgs<CompanyModel, CompanyModel> args)
    {
        if (args.Value is null)
            return;

        if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminCompany, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminCompany);

            return;
        }

        _selectedCompany = args.Value;
        _itemIssue.CompanyId = _selectedCompany.Id;

        await SaveTransactionFile();
    }

    private async Task OnGarageChanged(ChangeEventArgs<GarageModel?, GarageModel?> args)
    {
        if (args.Value is null)
        {
            _selectedGarage = null;
            _itemIssue.GarageId = null;
        }

        else if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminGarage, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminGarage);

            return;
        }

        else
        {
            _selectedGarage = args.Value;
            _itemIssue.GarageId = _selectedGarage.Id;

            foreach (var cartItem in _cart)
            {
                cartItem.VehicleId = null;
                cartItem.VehicleCode = null;
                cartItem.VehicleShortCode = null;
                cartItem.CurrentHour = null;
                cartItem.CurrentKM = null;
            }
        }

        await LoadItems();
        await SaveTransactionFile();
    }

    private async Task OnTransactionDateChanged(Syncfusion.Blazor.Calendars.ChangedEventArgs<DateTime> args)
    {
        _itemIssue.TransactionDateTime = args.Value;
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
                Quantity = 1,
                UnitOfMeasurement = "",
                Rate = 0
            };

        else
        {
            _selectedCart.ItemId = _selectedItem.Id;
            _selectedCart.ItemName = _selectedItem.Name;
            _selectedCart.Quantity = 1;
            _selectedCart.UnitOfMeasurement = _selectedItem.UnitOfMeasurement;
            _selectedCart.Rate = _selectedItem.Rate;
        }

        UpdateSelectedItemFinancialDetails();
    }

    private async Task OnVehicleChanged(ChangeEventArgs<VehicleModel?, VehicleModel> args)
    {
        if (args.Value is null || _itemIssue.GarageId is not null && _itemIssue.GarageId > 0)
        {
            _selectedCart.VehicleId = null;
            _selectedCart.VehicleCode = null;
            _selectedCart.VehicleShortCode = null;
            _selectedCart.CurrentHour = null;
            _selectedCart.CurrentKM = null;
            return;
        }

        if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminVehicle, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminVehicle);

            return;
        }

        _selectedCart.VehicleId = args.Value.Id;
        _selectedCart.VehicleCode = args.Value.Code;
        _selectedCart.VehicleShortCode = args.Value.ShortCode;
    }

    private void OnItemQuantityChanged(ChangeEventArgs<decimal> args)
    {
        _selectedCart.Quantity = args.Value;
        UpdateSelectedItemFinancialDetails();
    }

    private void OnItemRateChanged(ChangeEventArgs<decimal> args)
    {
        _selectedCart.Rate = args.Value;
        UpdateSelectedItemFinancialDetails();
    }

    private void UpdateSelectedItemFinancialDetails()
    {
        if (_selectedItem is null)
            return;

        if (_selectedCart.Quantity <= 0)
            _selectedCart.Quantity = 1;

        if (string.IsNullOrWhiteSpace(_selectedCart.UnitOfMeasurement))
            _selectedCart.UnitOfMeasurement = _selectedItem.UnitOfMeasurement;

        _selectedCart.ItemId = _selectedItem.Id;
        _selectedCart.ItemName = _selectedItem.Name;
        _selectedCart.Total = _selectedItem.Rate * _selectedCart.Quantity;

        StateHasChanged();
    }

    private async Task AddItemToCart()
    {
        if (_selectedItem is null || _selectedItem.Id <= 0 || _selectedCart.Quantity <= 0 || _selectedCart.Rate < 0 || _selectedCart.Total < 0 || string.IsNullOrEmpty(_selectedCart.UnitOfMeasurement))
        {
            await _toastNotification.ShowAsync("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", ToastType.Error);
            return;
        }

        if (_selectedCart.VehicleId is not null && _selectedCart.VehicleId > 0 && _selectedCart.CurrentHour is null && _selectedCart.CurrentKM is null)
        {
            await _toastNotification.ShowAsync("Vehicle Details Missing", "Please enter either the current hour or current KM for the selected vehicle.", ToastType.Error);
            return;
        }

        if (_itemIssue.GarageId is not null && _itemIssue.GarageId > 0)
        {
            _selectedCart.VehicleId = null;
            _selectedCart.VehicleCode = null;
            _selectedCart.VehicleShortCode = null;
            _selectedCart.CurrentHour = null;
            _selectedCart.CurrentKM = null;
        }

        if ((_itemIssue.GarageId is null || _itemIssue.GarageId == 0) && (_selectedCart.VehicleId is null || _selectedCart.VehicleId == 0))
        {
            await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the item issue when no garage is selected.", ToastType.Error);
            return;
        }

        UpdateSelectedItemFinancialDetails();

        var existingItem = _cart.FirstOrDefault(s => s.ItemId == _selectedCart.ItemId);
        if (existingItem is not null)
        {
            existingItem.Quantity += _selectedCart.Quantity;
            existingItem.Rate = _selectedCart.Rate;
        }
        else
            _cart.Add(new()
            {
                ItemId = _selectedCart.ItemId,
                ItemName = _selectedCart.ItemName,
                VehicleId = _selectedCart.VehicleId,
                VehicleShortCode = _selectedCart.VehicleId is not null ? _selectedCart.VehicleShortCode : null,
                VehicleCode = _selectedCart.VehicleId is not null ? _selectedCart.VehicleCode : null,
                CurrentHour = _selectedCart.VehicleId is not null ? _selectedCart.CurrentHour : null,
                CurrentKM = _selectedCart.VehicleId is not null ? _selectedCart.CurrentKM : null,
                IdentificationNo = _selectedCart.IdentificationNo,
                UnitOfMeasurement = _selectedCart.UnitOfMeasurement,
                Quantity = _selectedCart.Quantity,
                Rate = _selectedCart.Rate,
                Remarks = _selectedCart.Remarks
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

    private async Task EditCartItem(ItemIssueItemCartModel cartItem)
    {
        _selectedItem = _items.FirstOrDefault(s => s.Id == cartItem.ItemId);

        if (_selectedItem is null)
            return;

        _selectedCart = new()
        {
            ItemId = cartItem.ItemId,
            ItemName = cartItem.ItemName,
            VehicleId = cartItem.VehicleId,
            VehicleCode = cartItem.VehicleCode,
            VehicleShortCode = cartItem.VehicleShortCode,
            CurrentHour = cartItem.CurrentHour,
            CurrentKM = cartItem.CurrentKM,
            IdentificationNo = cartItem.IdentificationNo,
            UnitOfMeasurement = cartItem.UnitOfMeasurement,
            Quantity = cartItem.Quantity,
            Rate = cartItem.Rate,
            Remarks = cartItem.Remarks
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

    private async Task RemoveItemFromCart(ItemIssueItemCartModel cartItem)
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
            if (item.Quantity == 0)
                _cart.Remove(item);

            item.Total = item.Rate * item.Quantity;

            item.Remarks = item.Remarks?.Trim();
            if (string.IsNullOrWhiteSpace(item.Remarks))
                item.Remarks = null;

            item.IdentificationNo = item.IdentificationNo?.Trim();
            if (string.IsNullOrWhiteSpace(item.IdentificationNo))
                item.IdentificationNo = null;

            if (item.VehicleId is null || item.VehicleId == 0 || _itemIssue.GarageId is not null || _itemIssue.GarageId == 0)
            {
                item.VehicleId = null;
                item.VehicleCode = null;
                item.VehicleShortCode = null;
                item.CurrentHour = null;
                item.CurrentKM = null;
            }

            if ((_itemIssue.GarageId is null || _itemIssue.GarageId == 0) && (item.VehicleId is null || item.VehicleId == 0))
            {
                await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the item issue when no garage is selected.", ToastType.Error);
                return;
            }

            if (item.VehicleId is not null && item.VehicleId > 0 && item.CurrentHour is null && item.CurrentKM is null)
            {
                await _toastNotification.ShowAsync("Vehicle Details Missing", "Please enter either the current hour or current KM for the selected vehicle.", ToastType.Error);
                return;
            }
        }

        _itemIssue.TotalItems = _cart.Count;
        _itemIssue.TotalQuantity = _cart.Sum(x => x.Quantity);
        _itemIssue.TotalAmount = _cart.Sum(x => x.Total);
        _itemAfterTaxTotal = _cart.Sum(x => x.Total);

        _itemIssue.CompanyId = _selectedCompany.Id;
        _itemIssue.GarageId = _selectedGarage?.Id;
        _itemIssue.CreatedBy = _user.Id;

        #region Financial Year
        _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_itemIssue.TransactionDateTime);
        if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
            _itemIssue.FinancialYearId = _selectedFinancialYear.Id;
        else
        {
            await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
            _itemIssue.TransactionDateTime = await CommonData.LoadCurrentDateTime();
            _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_itemIssue.TransactionDateTime);
            _itemIssue.FinancialYearId = _selectedFinancialYear.Id;
        }
        #endregion

        if (Id is null)
            _itemIssue.TransactionNo = await GenerateCodes.GenerateItemIssueTransactionNo(_itemIssue);
    }

    private async Task SaveTransactionFile()
    {
        if (_isProcessing || _isLoading)
            return;

        try
        {
            _isProcessing = true;

            await UpdateFinancialDetails();

            await DataStorageService.LocalSaveAsync(StorageFileNames.ItemIssueDataFileName, System.Text.Json.JsonSerializer.Serialize(_itemIssue));
            await DataStorageService.LocalSaveAsync(StorageFileNames.ItemIssueCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cart));
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
        if (_selectedCompany is null || _itemIssue.CompanyId <= 0)
        {
            await _toastNotification.ShowAsync("Company Not Selected", "Please select a company for the transaction.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_itemIssue.TransactionNo))
        {
            await _toastNotification.ShowAsync("Transaction Number Missing", "Please enter a transaction number for the transaction.", ToastType.Warning);
            return false;
        }

        if (_itemIssue.TransactionDateTime == default)
        {
            await _toastNotification.ShowAsync("Transaction Date Missing", "Please select a valid transaction date for the transaction.", ToastType.Warning);
            return false;
        }

        if (_selectedFinancialYear is null || _itemIssue.FinancialYearId <= 0)
        {
            await _toastNotification.ShowAsync("Financial Year Not Found", "The transaction date does not fall within any financial year. Please check the date and try again.", ToastType.Error);
            return false;
        }

        if (_selectedFinancialYear.Locked)
        {
            await _toastNotification.ShowAsync("Financial Year Locked", "The financial year for the selected transaction date is locked. Please select a different date.", ToastType.Error);
            return false;
        }

        if (_selectedFinancialYear.Status == false)
        {
            await _toastNotification.ShowAsync("Financial Year Inactive", "The financial year for the selected transaction date is inactive. Please select a different date.", ToastType.Error);
            return false;
        }

        if (_itemIssue.TotalItems <= 0)
        {
            await _toastNotification.ShowAsync("No Items in Cart", "The transaction must contain at least one item in the cart.", ToastType.Warning);
            return false;
        }

        if (_itemIssue.TotalQuantity <= 0)
        {
            await _toastNotification.ShowAsync("Invalid Total Quantity", "The total quantity of the transaction must be greater than zero.", ToastType.Error);
            return false;
        }

        if (_itemIssue.TotalAmount < 0)
        {
            await _toastNotification.ShowAsync("Invalid Total Amount", "The total amount of the transaction must be greater than zero.", ToastType.Error);
            return false;
        }

        if (_cart.Any(item => item.Quantity <= 0))
        {
            await _toastNotification.ShowAsync("Invalid Item Quantity", "One or more items in the cart have a quantity less than or equal to zero. Please correct the quantities before saving.", ToastType.Error);
            return false;
        }

        if ((_itemIssue.GarageId is null || _itemIssue.GarageId == 0) && _cart.Any(item => item.VehicleId is null || item.VehicleId == 0))
        {
            await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the item issue when no garage is selected.", ToastType.Error);
            return false;
        }

        if (_itemIssue.GarageId is not null || _itemIssue.GarageId > 0)
            foreach (var item in _cart)
            {
                item.VehicleId = null;
                item.VehicleCode = null;
                item.VehicleShortCode = null;
                item.CurrentHour = null;
                item.CurrentKM = null;
            }

        if (_itemIssue.Id > 0)
        {
            var existingKitchenIssue = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, _itemIssue.Id);
            var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _itemIssue.FinancialYearId);
            if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            {
                await _toastNotification.ShowAsync("Financial Year Locked or Inactive", "The financial year for the selected transaction date is either locked or inactive. Please select a different date.", ToastType.Error);
                return false;
            }

            if (!_user.Admin)
            {
                await _toastNotification.ShowAsync("Insufficient Permissions", "You do not have the necessary permissions to modify this transaction.", ToastType.Error);
                return false;
            }
        }

        _itemIssue.Remarks = _itemIssue.Remarks?.Trim();
        if (string.IsNullOrWhiteSpace(_itemIssue.Remarks))
            _itemIssue.Remarks = null;

        return true;
    }

    private async Task SaveTransaction()
    {
        if (_isProcessing || _isLoading)
            return;

        try
        {
            _isProcessing = true;

            await SaveTransactionFile();

            if (!await ValidateForm())
            {
                _isProcessing = false;
                return;
            }

            await _toastNotification.ShowAsync("Processing Transaction", "Please wait while the transaction is being saved...", ToastType.Info);

            _itemIssue.Status = true;
            var currentDateTime = await CommonData.LoadCurrentDateTime();
            _itemIssue.TransactionDateTime = DateOnly.FromDateTime(_itemIssue.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
            _itemIssue.LastModifiedAt = currentDateTime;
            _itemIssue.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _itemIssue.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _itemIssue.CreatedBy = _user.Id;
            _itemIssue.LastModifiedBy = _user.Id;

            _itemIssue.Id = await ItemIssueData.SaveItemIssueTransaction(_itemIssue, _cart);
            var (pdfStream, fileName) = await ItemIssueInvoicePDFExport.ExportInvoice(_itemIssue.Id);
            await SaveAndViewService.SaveAndView(fileName, pdfStream);
            await DeleteLocalFiles();
            NavigationManager.NavigateTo(PageRouteNames.ItemIssue, true);

            await _toastNotification.ShowAsync("Save Transaction", "Transaction saved successfully! Invoice has been generated.", ToastType.Success);
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

    private async Task DeleteLocalFiles()
    {
        await DataStorageService.LocalRemove(StorageFileNames.ItemIssueDataFileName);
        await DataStorageService.LocalRemove(StorageFileNames.ItemIssueCartDataFileName);
    }
    #endregion

    #region Utilities
    private async Task ResetPage()
    {
        await DeleteLocalFiles();
        NavigationManager.NavigateTo(PageRouteNames.ItemIssue, true);
    }

    private async Task NavigateToTransactionHistoryPage()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportItemIssue, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportItemIssue);
    }

    private async Task NavigateToItemReport()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportGarageIssueItem, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportGarageIssueItem);
    }

    private async Task DownloadPdfInvoice()
    {
        if (!Id.HasValue || Id.Value <= 0)
        {
            await _toastNotification.ShowAsync("No Transaction Selected", "Please save the transaction first before downloading the invoice.", ToastType.Warning);
            return;
        }

        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Generating PDF invoice...", ToastType.Info);
            var (pdfStream, fileName) = await ItemIssueInvoicePDFExport.ExportInvoice(Id.Value);
            await SaveAndViewService.SaveAndView(fileName, pdfStream);
            await _toastNotification.ShowAsync("Invoice Downloaded", "The PDF invoice has been downloaded successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Downloading Invoice", ex.Message, ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task DownloadExcelInvoice()
    {
        if (!Id.HasValue || Id.Value <= 0)
        {
            await _toastNotification.ShowAsync("No Transaction Selected", "Please save the transaction first before downloading the invoice.", ToastType.Warning);
            return;
        }

        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Generating Excel invoice...", ToastType.Info);
            var (excelStream, fileName) = await ItemIssueInvoiceExcelExport.ExportInvoice(Id.Value);
            await SaveAndViewService.SaveAndView(fileName, excelStream);
            await _toastNotification.ShowAsync("Invoice Downloaded", "The Excel invoice has been downloaded successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Downloading Invoice", ex.Message, ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void NavigateToDashboard() =>
        NavigationManager.NavigateTo(PageRouteNames.Dashboard);

    private async Task NavigateBack() =>
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