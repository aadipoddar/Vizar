using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Repair;
using VizarLibrary.Data.Inventory.Purchase;
using VizarLibrary.Data.Inventory.Stock;
using VizarLibrary.Data.Operations;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Repair;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Stock;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Repair;

public partial class InsideRepairPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;

    [Parameter] public int? Id { get; set; }

    private UserModel _user;

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private CompanyModel _selectedCompany = new();
    private GarageModel _selectedGarage = new();
    private VehicleModel _selectedVehicle = new();
    private FinancialYearModel _selectedFinancialYear = new();
    private ItemModel? _selectedItem = new();
    private InsideRepairItemCartModel _selectedCart = new();
    private InsideRepairModel _insideRepair = new();

    private List<ItemStockSummaryModel> _stockSummary = [];
    private List<CompanyModel> _companies = [];
    private List<GarageModel> _garages = [];
    private List<ItemModel> _items = [];
    private List<VehicleModel> _vehicles = [];
    private List<InsideRepairItemCartModel> _cart = [];

    private SfAutoComplete<ItemModel?, ItemModel> _sfItemAutoComplete;
    private SfGrid<InsideRepairItemCartModel> _sfCartGrid;

    private ToastNotification _toastNotification;

    #region Load Data
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, UserRoles.Fleet);
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
        await LoadVehicles();
        await LoadExistingTransaction();
        await LoadItems();
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
            _garages.RemoveAll(s => s.External);
            _garages = [.. _garages.OrderBy(s => s.Name)];
            _selectedGarage = _garages.FirstOrDefault();
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

    private async Task LoadVehicles()
    {
        try
        {
            _vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(TableNames.Vehicle);
            _vehicles = [.. _vehicles.OrderBy(s => s.Code)];
            _selectedVehicle = _vehicles.FirstOrDefault();
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

    private async Task LoadExistingTransaction()
    {
        try
        {
            if (Id.HasValue)
            {
                _insideRepair = await CommonData.LoadTableDataById<InsideRepairModel>(TableNames.InsideRepair, Id.Value);
                if (_insideRepair is null)
                {
                    await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
                    NavigationManager.NavigateTo(PageRouteNames.InsideRepair, true);
                }
            }

            else if (await DataStorageService.LocalExists(StorageFileNames.InsideRepairDataFileName))
                _insideRepair = System.Text.Json.JsonSerializer.Deserialize<InsideRepairModel>(await DataStorageService.LocalGetAsync(StorageFileNames.InsideRepairDataFileName));

            else
            {
                _insideRepair = new()
                {
                    Id = 0,
                    TransactionNo = string.Empty,
                    CompanyId = _selectedCompany.Id,
                    GarageId = _selectedGarage.Id,
                    VehicleId = _selectedVehicle.Id,
                    CurrentHour = null,
                    CurrentKM = null,
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

            if (_insideRepair.CompanyId > 0)
                _selectedCompany = _companies.FirstOrDefault(s => s.Id == _insideRepair.CompanyId);
            else
            {
                _selectedCompany = _companies.FirstOrDefault();
                _insideRepair.CompanyId = _selectedCompany.Id;
            }

            if (_insideRepair.GarageId > 0)
                _selectedGarage = _garages.FirstOrDefault(s => s.Id == _insideRepair.GarageId);
            else
            {
                _selectedGarage = null;
                _insideRepair.GarageId = _selectedGarage.Id;
            }

            if (_insideRepair.VehicleId > 0)
                _selectedVehicle = _vehicles.FirstOrDefault(s => s.Id == _insideRepair.VehicleId);
            else
            {
                _selectedVehicle = null;
                _insideRepair.VehicleId = _selectedVehicle.Id;
            }

            _selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _insideRepair.FinancialYearId);
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
            _items = await PurchaseData.LoadItemByVendorPurchaseDateTime(0, _insideRepair.TransactionDateTime);
            _items = [.. _items.OrderBy(s => s.Name)];
            _items.Add(new()
            {
                Id = 0,
                Name = "Create New Item ..."
            });

            _stockSummary = await ItemStockData.LoadItemStockSummaryByGarageDate(_selectedGarage.Id, _insideRepair.TransactionDateTime, _insideRepair.TransactionDateTime);
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

            if (_insideRepair.Id > 0)
            {
                var existingCart = await CommonData.LoadTableDataByMasterId<InsideRepairDetailModel>(TableNames.InsideRepairDetail, _insideRepair.Id);

                foreach (var item in existingCart)
                {
                    if (_items.FirstOrDefault(s => s.Id == item.ItemId) is null)
                    {
                        var rawMaterial = await CommonData.LoadTableDataById<ItemModel>(TableNames.Item, item.ItemId);
                        await _toastNotification.ShowAsync("Item Not Found", $"The item {rawMaterial?.Name} (ID: {item.ItemId}) in the existing transaction cart was not found in the available items list. It may have been deleted or is inaccessible.", ToastType.Error);
                        continue;
                    }

                    _cart.Add(new()
                    {
                        ItemId = item.ItemId,
                        ItemName = _items.FirstOrDefault(s => s.Id == item.ItemId)?.Name ?? "",
                        IdentificationNo = item.IdentificationNo,
                        UnitOfMeasurement = item.UnitOfMeasurement,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        Total = item.Total,
                        Remarks = item.Remarks
                    });
                }
            }

            else if (await DataStorageService.LocalExists(StorageFileNames.InsideRepairCartDataFileName))
                _cart = System.Text.Json.JsonSerializer.Deserialize<List<InsideRepairItemCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.InsideRepairCartDataFileName));
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
        _insideRepair.CompanyId = _selectedCompany.Id;

        await SaveTransactionFile();
    }

    private async Task OnGarageChanged(ChangeEventArgs<GarageModel, GarageModel> args)
    {
        if (args.Value is null)
            return;

        else if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminGarage, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminGarage);

            return;
        }

        _selectedGarage = args.Value;
        _insideRepair.GarageId = _selectedGarage.Id;

        await SaveTransactionFile();
    }

    private async Task OnVehicleChanged(ChangeEventArgs<VehicleModel, VehicleModel> args)
    {
        if (args.Value is null)
            return;

        else if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminVehicle, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminVehicle);

            return;
        }

        _selectedVehicle = args.Value;
        _insideRepair.VehicleId = _selectedVehicle.Id;

        await SaveTransactionFile();
    }

    private async Task OnTransactionDateChanged(Syncfusion.Blazor.Calendars.ChangedEventArgs<DateTime> args)
    {
        _insideRepair.TransactionDateTime = args.Value;
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

    private async Task EditCartItem(InsideRepairItemCartModel cartItem)
    {
        _selectedItem = _items.FirstOrDefault(s => s.Id == cartItem.ItemId);

        if (_selectedItem is null)
            return;

        _selectedCart = new()
        {
            ItemId = cartItem.ItemId,
            ItemName = cartItem.ItemName,
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

    private async Task RemoveItemFromCart(InsideRepairItemCartModel cartItem)
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
        }

        _insideRepair.TotalItems = _cart.Count;
        _insideRepair.TotalQuantity = _cart.Sum(x => x.Quantity);
        _insideRepair.TotalAmount = _cart.Sum(x => x.Total);

        _insideRepair.CompanyId = _selectedCompany.Id;
        _insideRepair.GarageId = _selectedGarage.Id;
        _insideRepair.VehicleId = _selectedVehicle.Id;
        _insideRepair.CreatedBy = _user.Id;

        #region Financial Year
        _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_insideRepair.TransactionDateTime);
        if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
            _insideRepair.FinancialYearId = _selectedFinancialYear.Id;
        else
        {
            await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
            _insideRepair.TransactionDateTime = await CommonData.LoadCurrentDateTime();
            _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_insideRepair.TransactionDateTime);
            _insideRepair.FinancialYearId = _selectedFinancialYear.Id;
        }
        #endregion

        if (Id is null)
            _insideRepair.TransactionNo = await GenerateCodes.GenerateInsideRepairTransactionNo(_insideRepair);
    }

    private async Task SaveTransactionFile()
    {
        if (_isProcessing || _isLoading)
            return;

        try
        {
            _isProcessing = true;

            await UpdateFinancialDetails();

            await DataStorageService.LocalSaveAsync(StorageFileNames.InsideRepairDataFileName, System.Text.Json.JsonSerializer.Serialize(_insideRepair));
            await DataStorageService.LocalSaveAsync(StorageFileNames.InsideRepairCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cart));
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
        if (_selectedCompany is null || _insideRepair.CompanyId <= 0)
        {
            await _toastNotification.ShowAsync("Company Not Selected", "Please select a company for the transaction.", ToastType.Warning);
            return false;
        }

        if (_selectedGarage is null || _insideRepair.GarageId <= 0)
        {
            await _toastNotification.ShowAsync("Garage Not Selected", "Please select a garage for the transaction.", ToastType.Warning);
            return false;
        }

        if (_selectedVehicle is null || _insideRepair.VehicleId <= 0)
        {
            await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the transaction.", ToastType.Warning);
            return false;
        }

        if ((_insideRepair.CurrentKM is null || _insideRepair.CurrentKM < 0) && (_insideRepair.CurrentHour is null || _insideRepair.CurrentHour < 0))
        {
            await _toastNotification.ShowAsync("Current KM/Hour Missing", "Please enter valid current KM and hour for the vehicle.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_insideRepair.TransactionNo))
        {
            await _toastNotification.ShowAsync("Transaction Number Missing", "Please enter a transaction number for the transaction.", ToastType.Warning);
            return false;
        }

        if (_insideRepair.TransactionDateTime == default)
        {
            await _toastNotification.ShowAsync("Transaction Date Missing", "Please select a valid transaction date for the transaction.", ToastType.Warning);
            return false;
        }

        if (_selectedFinancialYear is null || _insideRepair.FinancialYearId <= 0)
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

        if (_insideRepair.TotalItems <= 0)
        {
            await _toastNotification.ShowAsync("No Items in Cart", "The transaction must contain at least one item in the cart.", ToastType.Warning);
            return false;
        }

        if (_insideRepair.TotalQuantity <= 0)
        {
            await _toastNotification.ShowAsync("Invalid Total Quantity", "The total quantity of the transaction must be greater than zero.", ToastType.Error);
            return false;
        }

        if (_insideRepair.TotalAmount < 0)
        {
            await _toastNotification.ShowAsync("Invalid Total Amount", "The total amount of the transaction must be greater than zero.", ToastType.Error);
            return false;
        }

        if (_cart.Any(item => item.Quantity <= 0))
        {
            await _toastNotification.ShowAsync("Invalid Item Quantity", "One or more items in the cart have a quantity less than or equal to zero. Please correct the quantities before saving.", ToastType.Error);
            return false;
        }

        if (_insideRepair.Id > 0)
        {
            var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _insideRepair.FinancialYearId);
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

        _insideRepair.Remarks = _insideRepair.Remarks?.Trim();
        if (string.IsNullOrWhiteSpace(_insideRepair.Remarks))
            _insideRepair.Remarks = null;

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

            _insideRepair.Status = true;
            var currentDateTime = await CommonData.LoadCurrentDateTime();
            _insideRepair.TransactionDateTime = DateOnly.FromDateTime(_insideRepair.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
            _insideRepair.LastModifiedAt = currentDateTime;
            _insideRepair.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _insideRepair.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _insideRepair.CreatedBy = _user.Id;
            _insideRepair.LastModifiedBy = _user.Id;

            _insideRepair.Id = await InsideRepairData.SaveTransaction(_insideRepair, _cart);

            var (pdfStream, fileName) = await InsideRepairInvoiceExport.ExportInvoice(_insideRepair.Id, InvoiceExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, pdfStream);

            await ResetPage();

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
        await DataStorageService.LocalRemove(StorageFileNames.InsideRepairDataFileName);
        await DataStorageService.LocalRemove(StorageFileNames.InsideRepairCartDataFileName);
    }
    #endregion

    #region Utilities
    private async Task ResetPage()
    {
        await DeleteLocalFiles();
        NavigationManager.NavigateTo(PageRouteNames.InsideRepair, true);
    }

    private async Task NavigateToTransactionHistoryPage()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportInsideRepair, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportInsideRepair);
    }

    private async Task NavigateToItemReport()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportInsideRepairItem, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportInsideRepairItem);
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

            var (pdfStream, fileName) = await InsideRepairInvoiceExport.ExportInvoice(_insideRepair.Id, InvoiceExportType.PDF);
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

            var (excelStream, fileName) = await InsideRepairInvoiceExport.ExportInvoice(_insideRepair.Id, InvoiceExportType.Excel);
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

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

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