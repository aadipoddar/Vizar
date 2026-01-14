using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Service;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Service;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;

namespace Vizar.Shared.Pages.Fleet.Service;

public partial class ServicePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;

    [Parameter] public int? Id { get; set; }

    private UserModel _user;

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private decimal _itemAfterTaxTotal = 0;

    private CompanyModel _selectedCompany = new();
    private GarageModel _selectedGarage = new();
    private FinancialYearModel _selectedFinancialYear = new();
    private ServiceTypeModel? _selectedServiceType = new();
    private VehicleServiceItemOverviewModel? _lastVehicleService = null;
    private ServiceItemCartModel _selectedCart = new();
    private ServiceModel _service = new();

    private List<CompanyModel> _companies = [];
    private List<GarageModel> _garages = [];
    private List<ServiceTypeModel> _serviceTypes = [];
    private List<VehicleModel> _vehicles = [];
    private List<ServiceItemCartModel> _cart = [];

    private SfAutoComplete<ServiceTypeModel?, ServiceTypeModel> _sfItemAutoComplete;
    private SfGrid<ServiceItemCartModel> _sfCartGrid;

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
            .Add(ModCode.Alt, Code.G, NavigateToGarageItemReport, "Open garage item report", Exclude.None)
            .Add(ModCode.Alt, Code.V, NavigateToVehicleItemReport, "Open vehicle item report", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Go to dashboard", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(Code.Delete, RemoveSelectedCartItem, "Delete selected cart item", Exclude.None)
            .Add(Code.Insert, EditSelectedCartItem, "Edit selected cart item", Exclude.None);

        await LoadCompanies();
        await LoadGarages();
        await LoadExistingTransaction();
        await LoadServiceTypes();
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

            _selectedGarage = _garages.FirstOrDefault();
            _service.GarageId = _selectedGarage.Id;
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
                _service = await CommonData.LoadTableDataById<ServiceModel>(TableNames.Service, Id.Value);
                if (_service is null)
                {
                    await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
                    NavigationManager.NavigateTo(PageRouteNames.Service, true);
                }
            }

            else if (await DataStorageService.LocalExists(StorageFileNames.ServiceDataFileName))
                _service = System.Text.Json.JsonSerializer.Deserialize<ServiceModel>(await DataStorageService.LocalGetAsync(StorageFileNames.ServiceDataFileName));

            else
            {
                _service = new()
                {
                    Id = 0,
                    TransactionNo = string.Empty,
                    CompanyId = _selectedCompany.Id,
                    GarageId = _garages.FirstOrDefault().Id,
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

            if (_service.CompanyId > 0)
                _selectedCompany = _companies.FirstOrDefault(s => s.Id == _service.CompanyId);
            else
            {
                _selectedCompany = _companies.FirstOrDefault();
                _service.CompanyId = _selectedCompany.Id;
            }

            if (_service.GarageId > 0)
                _selectedGarage = _garages.FirstOrDefault(s => s.Id == _service.GarageId);
            else
            {
                _selectedGarage = _garages.FirstOrDefault();
                _service.GarageId = _selectedGarage.Id;
            }

            _selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _service.FinancialYearId);
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

    private async Task LoadServiceTypes()
    {
        try
        {
            _serviceTypes = await CommonData.LoadTableDataByStatus<ServiceTypeModel>(TableNames.ServiceType);
            _serviceTypes = [.. _serviceTypes.OrderBy(s => s.Name)];
            _serviceTypes.Add(new()
            {
                Id = 0,
                Name = "Create New Service Type ..."
            });
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("An Error Occurred While Loading Service Types", ex.Message, ToastType.Error);
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

            _selectedCart.VehicleId = -1;
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

            if (_service.Id > 0)
            {
                var existingCart = await CommonData.LoadTableDataByMasterId<ServiceDetailModel>(TableNames.ServiceDetail, _service.Id);

                foreach (var serviceType in existingCart)
                {
                    if (_serviceTypes.FirstOrDefault(s => s.Id == serviceType.ServiceTypeId) is null)
                    {
                        var selectedServiceType = await CommonData.LoadTableDataById<ServiceTypeModel>(TableNames.ServiceType, serviceType.ServiceTypeId);
                        await _toastNotification.ShowAsync("Service Type Not Found", $"The service type {selectedServiceType?.Name} (ID: {serviceType.ServiceTypeId}) in the existing transaction cart was not found in the available Service Type list. It may have been deleted or is inaccessible.", ToastType.Error);
                        continue;
                    }

                    if (_vehicles.FirstOrDefault(s => s.Id == serviceType.VehicleId) is null)
                    {
                        var vehicle = await CommonData.LoadTableDataById<VehicleModel>(TableNames.Vehicle, serviceType.VehicleId);
                        await _toastNotification.ShowAsync("Vehicle Not Found", $"The vehicle {vehicle?.Code} (ID: {serviceType.VehicleId}) associated with the service type {_serviceTypes.FirstOrDefault(s => s.Id == serviceType.ServiceTypeId)?.Name ?? "Unknown Item"} in the existing transaction cart was not found in the available vehicles list. It may have been deleted or is inaccessible.", ToastType.Error);
                        continue;
                    }

                    _cart.Add(new()
                    {
                        ServiceTypeId = serviceType.ServiceTypeId,
                        ServiceTypeName = _serviceTypes.FirstOrDefault(s => s.Id == serviceType.ServiceTypeId)?.Name ?? "",
                        VehicleId = serviceType.VehicleId,
                        VehicleShortCode = _vehicles.FirstOrDefault(s => s.Id == serviceType.VehicleId)?.ShortCode,
                        VehicleCode = _vehicles.FirstOrDefault(s => s.Id == serviceType.VehicleId)?.Code,
                        CurrentHour = serviceType.CurrentHour,
                        CurrentKM = serviceType.CurrentKM,
                        Quantity = serviceType.Quantity,
                        Rate = serviceType.Rate,
                        Total = serviceType.Total,
                        Remarks = serviceType.Remarks
                    });
                }
            }

            else if (await DataStorageService.LocalExists(StorageFileNames.ServiceCartDataFileName))
                _cart = System.Text.Json.JsonSerializer.Deserialize<List<ServiceItemCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.ServiceCartDataFileName));
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
        _service.CompanyId = _selectedCompany.Id;

        await SaveTransactionFile();
    }

    private async Task OnGarageChanged(ChangeEventArgs<GarageModel, GarageModel> args)
    {
        if (args.Value is null)
            return;

        if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminGarage, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminGarage);

            return;
        }

        _selectedGarage = args.Value;
        _service.GarageId = _selectedGarage.Id;

        await LoadServiceTypes();
        await SaveTransactionFile();
    }

    private async Task OnTransactionDateChanged(Syncfusion.Blazor.Calendars.ChangedEventArgs<DateTime> args)
    {
        _service.TransactionDateTime = args.Value;
        await LoadServiceTypes();
        await SaveTransactionFile();
    }
    #endregion

    #region Cart
    private async Task OnServiceTypeChanged(ChangeEventArgs<ServiceTypeModel?, ServiceTypeModel> args)
    {
        if (args.Value is null)
            return;

        if (args.Value.Id == 0)
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminServiceType, "_blank");
            else
                NavigationManager.NavigateTo(PageRouteNames.AdminServiceType);

            return;
        }

        _selectedServiceType = args.Value;
        _selectedCart.ServiceTypeId = _selectedServiceType.Id;
        _selectedCart.ServiceTypeName = _selectedServiceType.Name;
        _selectedCart.Quantity = 1;
        _selectedCart.Rate = _selectedServiceType.Rate;
        await UpdateItemFinancialDetails();
    }

    private async Task OnVehicleChanged(ChangeEventArgs<VehicleModel, VehicleModel> args)
    {
        if (args.Value is null)
            return;

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
        await UpdateItemFinancialDetails();
    }

    private async Task OnServiceTypeQuantityChanged(ChangeEventArgs<decimal> args)
    {
        _selectedCart.Quantity = args.Value;
        await UpdateItemFinancialDetails();
    }

    private async Task OnServiceTypeRateChanged(ChangeEventArgs<decimal> args)
    {
        _selectedCart.Rate = args.Value;
        await UpdateItemFinancialDetails();
    }

    private async Task UpdateItemFinancialDetails()
    {
        if (_selectedServiceType is null)
            return;

        if (_selectedCart.Quantity <= 0)
            _selectedCart.Quantity = 1;

        _selectedCart.ServiceTypeId = _selectedServiceType.Id;
        _selectedCart.ServiceTypeName = _selectedServiceType.Name;
        _selectedCart.Total = _selectedServiceType.Rate * _selectedCart.Quantity;

        _selectedCart.VehicleCode = _vehicles.FirstOrDefault(v => v.Id == _selectedCart.VehicleId)?.Code;
        _selectedCart.VehicleShortCode = _vehicles.FirstOrDefault(v => v.Id == _selectedCart.VehicleId)?.ShortCode;
        _lastVehicleService = await ServiceData.LoadLastVehicleServiceItemByVehicleServiceTypeDate(_selectedCart.VehicleId, _selectedCart.ServiceTypeId, _service.TransactionDateTime);

        StateHasChanged();
    }

    private async Task AddItemToCart()
    {
        if (_selectedServiceType is null || _selectedServiceType.Id <= 0 || _selectedCart.Quantity <= 0 || _selectedCart.Rate < 0 || _selectedCart.Total < 0)
        {
            await _toastNotification.ShowAsync("Invalid Service Type Details", "Please ensure all service type details are correctly filled before adding to the cart.", ToastType.Error);
            return;
        }

        if (_selectedCart.VehicleId > 0 && _selectedCart.CurrentHour is null && _selectedCart.CurrentKM is null)
        {
            await _toastNotification.ShowAsync("Vehicle Details Missing", "Please enter either the current hour or current KM for the selected vehicle.", ToastType.Error);
            return;
        }

        if (_selectedCart.VehicleId <= 0)
        {
            await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the service.", ToastType.Error);
            return;
        }

        await UpdateItemFinancialDetails();

        var existingServiceType = _cart.FirstOrDefault(s => s.ServiceTypeId == _selectedCart.ServiceTypeId && s.VehicleId == _selectedCart.VehicleId);
        if (existingServiceType is not null)
        {
            existingServiceType.Quantity += _selectedCart.Quantity;
            existingServiceType.Rate = _selectedCart.Rate;
        }
        else
            _cart.Add(new()
            {
                ServiceTypeId = _selectedCart.ServiceTypeId,
                ServiceTypeName = _selectedCart.ServiceTypeName,
                VehicleId = _selectedCart.VehicleId,
                VehicleCode = _selectedCart.VehicleCode,
                VehicleShortCode = _selectedCart.VehicleShortCode,
                CurrentHour = _selectedCart.CurrentHour,
                CurrentKM = _selectedCart.CurrentKM,
                Quantity = _selectedCart.Quantity,
                Rate = _selectedCart.Rate,
                Remarks = _selectedCart.Remarks
            });

        _selectedServiceType = null;
        _selectedCart = new()
        {
            VehicleId = -1
        };

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

    private async Task EditCartItem(ServiceItemCartModel cartItem)
    {
        _selectedServiceType = _serviceTypes.FirstOrDefault(s => s.Id == cartItem.ServiceTypeId);

        if (_selectedServiceType is null)
            return;

        _selectedCart = new()
        {
            ServiceTypeId = cartItem.ServiceTypeId,
            ServiceTypeName = cartItem.ServiceTypeName,
            VehicleId = cartItem.VehicleId,
            VehicleCode = cartItem.VehicleCode,
            VehicleShortCode = cartItem.VehicleShortCode,
            CurrentHour = cartItem.CurrentHour,
            CurrentKM = cartItem.CurrentKM,
            Quantity = cartItem.Quantity,
            Rate = cartItem.Rate,
            Remarks = cartItem.Remarks
        };

        await _sfItemAutoComplete.FocusAsync();
        await UpdateItemFinancialDetails();
        await RemoveItemFromCart(cartItem);
    }

    private async Task RemoveSelectedCartItem()
    {
        if (_sfCartGrid is null || _sfCartGrid.SelectedRecords is null || _sfCartGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfCartGrid.SelectedRecords.First();
        await RemoveItemFromCart(selectedCartItem);
    }

    private async Task RemoveItemFromCart(ServiceItemCartModel cartItem)
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

            if (item.VehicleId == 0)
            {
                await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the service.", ToastType.Error);
                return;
            }

            if (item.VehicleId > 0 && item.CurrentHour is null && item.CurrentKM is null)
            {
                await _toastNotification.ShowAsync("Vehicle Details Missing", "Please enter either the current hour or current KM for the selected vehicle.", ToastType.Error);
                return;
            }

            item.VehicleCode = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId)?.Code;
            item.VehicleShortCode = _vehicles.FirstOrDefault(v => v.Id == item.VehicleId)?.ShortCode;
        }

        _service.TotalItems = _cart.Count;
        _service.TotalQuantity = _cart.Sum(x => x.Quantity);
        _service.TotalAmount = _cart.Sum(x => x.Total);
        _itemAfterTaxTotal = _cart.Sum(x => x.Total);

        _service.CompanyId = _selectedCompany.Id;
        _service.GarageId = _selectedGarage.Id;
        _service.CreatedBy = _user.Id;

        #region Financial Year
        _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_service.TransactionDateTime);
        if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
            _service.FinancialYearId = _selectedFinancialYear.Id;
        else
        {
            await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
            _service.TransactionDateTime = await CommonData.LoadCurrentDateTime();
            _selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_service.TransactionDateTime);
            _service.FinancialYearId = _selectedFinancialYear.Id;
        }
        #endregion

        if (Id is null)
            _service.TransactionNo = await GenerateCodes.GenerateServiceTransactionNo(_service);
    }

    private async Task SaveTransactionFile()
    {
        if (_isProcessing || _isLoading)
            return;

        try
        {
            _isProcessing = true;

            await UpdateFinancialDetails();

            await DataStorageService.LocalSaveAsync(StorageFileNames.ServiceDataFileName, System.Text.Json.JsonSerializer.Serialize(_service));
            await DataStorageService.LocalSaveAsync(StorageFileNames.ServiceCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cart));
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
        if (_selectedCompany is null || _service.CompanyId <= 0)
        {
            await _toastNotification.ShowAsync("Company Not Selected", "Please select a company for the transaction.", ToastType.Warning);
            return false;
        }

        if (_selectedGarage is null || _service.GarageId <= 0)
        {
            await _toastNotification.ShowAsync("Garage Not Selected", "Please select a garage for the transaction.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_service.TransactionNo))
        {
            await _toastNotification.ShowAsync("Transaction Number Missing", "Please enter a transaction number for the transaction.", ToastType.Warning);
            return false;
        }

        if (_service.TransactionDateTime == default)
        {
            await _toastNotification.ShowAsync("Transaction Date Missing", "Please select a valid transaction date for the transaction.", ToastType.Warning);
            return false;
        }

        if (_selectedFinancialYear is null || _service.FinancialYearId <= 0)
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

        if (_service.TotalItems <= 0)
        {
            await _toastNotification.ShowAsync("No Items in Cart", "The transaction must contain at least one item in the cart.", ToastType.Warning);
            return false;
        }

        if (_service.TotalQuantity <= 0)
        {
            await _toastNotification.ShowAsync("Invalid Total Quantity", "The total quantity of the transaction must be greater than zero.", ToastType.Error);
            return false;
        }

        if (_service.TotalAmount < 0)
        {
            await _toastNotification.ShowAsync("Invalid Total Amount", "The total amount of the transaction must be greater than zero.", ToastType.Error);
            return false;
        }

        if (_cart.Any(item => item.Quantity <= 0))
        {
            await _toastNotification.ShowAsync("Invalid Item Quantity", "One or more items in the cart have a quantity less than or equal to zero. Please correct the quantities before saving.", ToastType.Error);
            return false;
        }

        if (_cart.Any(item => item.VehicleId <= 0))
        {
            await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the transaction.", ToastType.Error);
            return false;
        }

        if (_service.Id > 0)
        {
            var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _service.FinancialYearId);
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

        _service.Remarks = _service.Remarks?.Trim();
        if (string.IsNullOrWhiteSpace(_service.Remarks))
            _service.Remarks = null;

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

            _service.Status = true;
            var currentDateTime = await CommonData.LoadCurrentDateTime();
            _service.TransactionDateTime = DateOnly.FromDateTime(_service.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
            _service.LastModifiedAt = currentDateTime;
            _service.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _service.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _service.CreatedBy = _user.Id;
            _service.LastModifiedBy = _user.Id;

            _service.Id = await ServiceData.SaveTransaction(_service, _cart);
            var (pdfStream, fileName) = await ServiceInvoicePDFExport.ExportInvoice(_service.Id);
            await SaveAndViewService.SaveAndView(fileName, pdfStream);
            await DeleteLocalFiles();
            NavigationManager.NavigateTo(PageRouteNames.Service, true);

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
        await DataStorageService.LocalRemove(StorageFileNames.ServiceDataFileName);
        await DataStorageService.LocalRemove(StorageFileNames.ServiceCartDataFileName);
    }
    #endregion

    #region Utilities
    private async Task ResetPage()
    {
        await DeleteLocalFiles();
        NavigationManager.NavigateTo(PageRouteNames.Service, true);
    }

    private async Task NavigateToTransactionHistoryPage()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportService, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportService);
    }

    private async Task NavigateToGarageItemReport()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportGarageServiceItem, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportGarageServiceItem);
    }

    private async Task NavigateToVehicleItemReport()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportVehicleServiceItem, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportVehicleServiceItem);
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
            var (pdfStream, fileName) = await ServiceInvoicePDFExport.ExportInvoice(Id.Value);
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
            var (excelStream, fileName) = await ServiceInvoiceExcelExport.ExportInvoice(Id.Value);
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