using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;

using Vizar.Shared.Services;

using VizarLibrary.Data;
using VizarLibrary.Data.Accounts;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory;
using VizarLibrary.Data.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory;
using VizarLibrary.Models.Item;

namespace Vizar.Shared.Pages.Inventory;

public partial class PurchasePage
{
	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _autoGenerateTransactionNo = false;

	private decimal _itemBaseTotal = 0;
	private decimal _itemDiscountTotal = 0;
	private decimal _itemAfterDiscountTotal = 0;
	private decimal _itemTaxTotal = 0;
	private decimal _itemAfterTaxTotal = 0;

	private CompanyModel _selectedCompany = new();
	private LedgerModel _selectedParty = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private ItemModel? _selectedItem = new();
	private PurchaseItemCartModel _selectedCart = new();
	private PurchaseModel _purchase = new();

	private List<CompanyModel> _companies = [];
	private List<LedgerModel> _parties = [];
	private List<ItemModel> _items = [];
	private List<TaxModel> _taxes = [];
	private List<PurchaseItemCartModel> _cart = [];

	private SfAutoComplete<ItemModel?, ItemModel> _sfItemAutoComplete;
	private SfGrid<PurchaseItemCartModel> _sfCartGrid;

	private string _errorTitle = string.Empty;
	private string _errorMessage = string.Empty;

	private string _successTitle = string.Empty;
	private string _successMessage = string.Empty;

	private SfToast _sfSuccessToast;
	private SfToast _sfErrorToast;

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
		await LoadCompanies();
		await LoadLedgers();
		await LoadExistingPurchase();
		await LoadItems();
		await LoadExistingCart();
		await SavePurchaseFile();
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
			await ShowToast("An Error Occurred While Loading Companies", ex.Message, "error");
		}
	}

	private async Task LoadLedgers()
	{
		try
		{
			_parties = await CommonData.LoadTableDataByStatus<LedgerModel>(TableNames.Ledger);
			_parties = [.. _parties.OrderBy(s => s.Name)];
			_parties.Add(new()
			{
				Id = 0,
				Name = "Create New Party Ledger..."
			});

			_selectedParty = _parties.FirstOrDefault();
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Loading Ledgers", ex.Message, "error");
		}
	}

	private async Task LoadExistingPurchase()
	{
		try
		{
			if (await DataStorageService.LocalExists(StorageFileNames.PurchaseDataFileName))
				_purchase = System.Text.Json.JsonSerializer.Deserialize<PurchaseModel>(await DataStorageService.LocalGetAsync(StorageFileNames.PurchaseDataFileName));
			else
			{
				_purchase = new()
				{
					Id = 0,
					TransactionNo = string.Empty,
					CompanyId = _selectedCompany.Id,
					PartyId = _selectedParty.Id,
					TransactionDateTime = await CommonData.LoadCurrentDateTime(),
					FinancialYearId = (await FinancialYearData.LoadFinancialYearByDateTime(await CommonData.LoadCurrentDateTime())).Id,
					UserId = _user.Id,
					ItemsTotalAmount = 0,
					CashDiscountPercent = 0,
					CashDiscountAmount = 0,
					OtherChargesPercent = 0,
					OtherChargesAmount = 0,
					RoundOffAmount = 0,
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

			if (_purchase.CompanyId > 0)
				_selectedCompany = _companies.FirstOrDefault(s => s.Id == _purchase.CompanyId);
			else
			{
				_selectedCompany = _companies.FirstOrDefault();
				_purchase.CompanyId = _selectedCompany.Id;
			}

			if (_purchase.PartyId > 0)
				_selectedParty = _parties.FirstOrDefault(s => s.Id == _purchase.PartyId);
			else
			{
				_selectedParty = _parties.FirstOrDefault();
				_purchase.PartyId = _selectedParty.Id;
			}

			_selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _purchase.FinancialYearId);
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Loading Purchase Data", ex.Message, "error");
			await DeleteLocalFiles();
		}
	}

	private async Task LoadItems()
	{
		try
		{
			_items = await ItemData.LoadItemByPartyPurchaseDateTime(_purchase.PartyId, _purchase.TransactionDateTime);
			_taxes = await CommonData.LoadTableDataByStatus<TaxModel>(TableNames.Tax);

			_items = [.. _items.OrderBy(s => s.Name)];
			_items.Add(new()
			{
				Id = 0,
				Name = "Create New Item ..."
			});
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Loading Items", ex.Message, "error");
		}
	}

	private async Task LoadExistingCart()
	{
		try
		{
			_cart.Clear();

			if (await DataStorageService.LocalExists(StorageFileNames.PurchaseCartDataFileName))
				_cart = System.Text.Json.JsonSerializer.Deserialize<List<PurchaseItemCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.PurchaseCartDataFileName));
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Loading Existing Cart", ex.Message, "error");
			await DeleteLocalFiles();
		}
	}
	#endregion

	#region Change Events
	private async Task OnCompanyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<CompanyModel, CompanyModel> args)
	{
		if (args.Value is null)
			return;

		if (args.Value.Id == 0)
		{
			if (FormFactor.GetFormFactor() == "Web")
				await JSRuntime.InvokeVoidAsync("open", "/Admin/Company", "_blank");
			else
				NavigationManager.NavigateTo("/Admin/Company");

			return;
		}

		_selectedCompany = args.Value;
		_purchase.CompanyId = _selectedCompany.Id;

		await SavePurchaseFile();
	}

	private async Task OnPartyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<LedgerModel, LedgerModel> args)
	{
		if (args.Value is null)
			return;

		if (args.Value.Id == 0)
		{
			if (FormFactor.GetFormFactor() == "Web")
				await JSRuntime.InvokeVoidAsync("open", "/Admin/Ledger", "_blank");
			else
				NavigationManager.NavigateTo("/Admin/Ledger");

			return;
		}

		_selectedParty = args.Value;
		_purchase.PartyId = _selectedParty.Id;

		await LoadItems();
		await SavePurchaseFile();
	}

	private async Task OnTransactionDateChanged(Syncfusion.Blazor.Calendars.ChangedEventArgs<DateTime> args)
	{
		_purchase.TransactionDateTime = args.Value;
		await LoadItems();
		await SavePurchaseFile();
	}

	private async Task OnAutoGenerateTransactionNoChecked(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
	{
		_autoGenerateTransactionNo = args.Checked;
		await SavePurchaseFile();
	}

	private async Task OnCashDiscountPercentChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_purchase.CashDiscountPercent = args.Value;
		await SavePurchaseFile();
	}

	private async Task OnOtherDiscountPercentChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_purchase.OtherChargesPercent = args.Value;
		await SavePurchaseFile();
	}

	private async Task OnRoundOffAmountChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_purchase.RoundOffAmount = args.Value;
		await SavePurchaseFile(true);
	}
	#endregion

	#region Cart
	private async Task OnItemChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ItemModel?, ItemModel> args)
	{
		if (args.Value is null)
			return;

		if (args.Value.Id == 0)
		{
			if (FormFactor.GetFormFactor() == "Web")
				await JSRuntime.InvokeVoidAsync("open", "/Admin/Item", "_blank");
			else
				NavigationManager.NavigateTo("/Admin/Item");

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
				Rate = 0,
				DiscountPercent = 0,
				CGSTPercent = 0,
				SGSTPercent = 0,
				IGSTPercent = 0
			};

		else
		{
			_selectedCart.ItemId = _selectedItem.Id;
			_selectedCart.ItemName = _selectedItem.Name;
			_selectedCart.Quantity = 1;
			_selectedCart.UnitOfMeasurement = _selectedItem.UnitOfMeasurement;
			_selectedCart.Rate = _selectedItem.Rate;
			_selectedCart.DiscountPercent = 0;
			_selectedCart.CGSTPercent = _taxes.FirstOrDefault(s => s.Id == _selectedItem.TaxId).CGST;
			_selectedCart.SGSTPercent = _taxes.FirstOrDefault(s => s.Id == _selectedItem.TaxId).SGST;
			_selectedCart.IGSTPercent = _taxes.FirstOrDefault(s => s.Id == _selectedItem.TaxId).IGST;
		}

		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemQuantityChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_selectedCart.Quantity = args.Value;
		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemRateChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_selectedCart.Rate = args.Value;
		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemDiscountPercentChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_selectedCart.DiscountPercent = args.Value;
		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemCGSTPercentChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_selectedCart.CGSTPercent = args.Value;
		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemSGSTPercentChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_selectedCart.SGSTPercent = args.Value;
		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemIGSTPercentChanged(Syncfusion.Blazor.Inputs.ChangeEventArgs<decimal> args)
	{
		_selectedCart.IGSTPercent = args.Value;
		UpdateSelectedItemFinancialDetails();
	}

	private void OnItemInclusiveTaxChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
	{
		_selectedCart.InclusiveTax = args.Checked;
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
		_selectedCart.BaseTotal = _selectedCart.Rate * _selectedCart.Quantity;
		_selectedCart.DiscountAmount = _selectedCart.BaseTotal * (_selectedCart.DiscountPercent / 100);
		_selectedCart.AfterDiscount = _selectedCart.BaseTotal - _selectedCart.DiscountAmount;
		_selectedCart.CGSTAmount = _selectedCart.AfterDiscount * (_selectedCart.CGSTPercent / 100);
		_selectedCart.SGSTAmount = _selectedCart.AfterDiscount * (_selectedCart.SGSTPercent / 100);
		_selectedCart.IGSTAmount = _selectedCart.AfterDiscount * (_selectedCart.IGSTPercent / 100);
		_selectedCart.TotalTaxAmount = _selectedCart.CGSTAmount + _selectedCart.SGSTAmount + _selectedCart.IGSTAmount;
		_selectedCart.Total = _selectedCart.InclusiveTax ? _selectedCart.AfterDiscount : _selectedCart.AfterDiscount + _selectedCart.TotalTaxAmount;

		StateHasChanged();
	}

	private async Task AddItemToCart()
	{
		if (_selectedItem is null || _selectedItem.Id <= 0 || _selectedCart.Quantity <= 0 || _selectedCart.Rate < 0 || _selectedCart.DiscountPercent < 0 || _selectedCart.CGSTPercent < 0 || _selectedCart.SGSTPercent < 0 || _selectedCart.IGSTPercent < 0 || _selectedCart.Total < 0)
		{
			await ShowToast("Invalid Item Details", "Please ensure all item details are correctly filled before adding to the cart.", "error");
			return;
		}

		UpdateSelectedItemFinancialDetails();

		var existingItem = _cart.FirstOrDefault(s => s.ItemId == _selectedCart.ItemId);
		if (existingItem is not null)
		{
			existingItem.Quantity += _selectedCart.Quantity;
			existingItem.Rate = _selectedCart.Rate;
			existingItem.DiscountPercent = _selectedCart.DiscountPercent;
			existingItem.CGSTPercent = _selectedCart.CGSTPercent;
			existingItem.SGSTPercent = _selectedCart.SGSTPercent;
			existingItem.IGSTPercent = _selectedCart.IGSTPercent;
		}
		else
			_cart.Add(new()
			{
				ItemId = _selectedCart.ItemId,
				ItemName = _selectedCart.ItemName,
				IdentificationNo = _selectedCart.IdentificationNo,
				Quantity = _selectedCart.Quantity,
				UnitOfMeasurement = _selectedCart.UnitOfMeasurement,
				Rate = _selectedCart.Rate,
				DiscountPercent = _selectedCart.DiscountPercent,
				CGSTPercent = _selectedCart.CGSTPercent,
				SGSTPercent = _selectedCart.SGSTPercent,
				IGSTPercent = _selectedCart.IGSTPercent,
				InclusiveTax = _selectedCart.InclusiveTax,
				Remarks = _selectedCart.Remarks
			});

		_selectedItem = null;
		_selectedCart = new();

		await _sfItemAutoComplete.FocusAsync();
		await SavePurchaseFile();
	}

	private async Task EditCartItem(PurchaseItemCartModel cartItem)
	{
		_selectedItem = _items.FirstOrDefault(s => s.Id == cartItem.ItemId);

		if (_selectedItem is null)
			return;

		_selectedCart = new()
		{
			ItemId = cartItem.ItemId,
			ItemName = cartItem.ItemName,
			IdentificationNo = cartItem.IdentificationNo,
			Quantity = cartItem.Quantity,
			UnitOfMeasurement = cartItem.UnitOfMeasurement,
			Rate = cartItem.Rate,
			DiscountPercent = cartItem.DiscountPercent,
			CGSTPercent = cartItem.CGSTPercent,
			SGSTPercent = cartItem.SGSTPercent,
			IGSTPercent = cartItem.IGSTPercent,
			InclusiveTax = cartItem.InclusiveTax,
			Remarks = cartItem.Remarks
		};

		await _sfItemAutoComplete.FocusAsync();
		UpdateSelectedItemFinancialDetails();
		await RemoveItemFromCart(cartItem);
	}

	private async Task RemoveItemFromCart(PurchaseItemCartModel cartItem)
	{
		_cart.Remove(cartItem);
		await SavePurchaseFile();
	}

	private async Task ClearCart()
	{
		if (_cart.Count == 0)
		{
			await ShowToast("Cart Empty", "The cart is already empty.", "error");
			return;
		}

		_cart.Clear();
		_selectedItem = null;
		_selectedCart = new();
		await SavePurchaseFile();
		await ShowToast("Cart Cleared", "All items have been removed from the cart.", "success");
	}
	#endregion

	#region Saving
	private async Task UpdateFinancialDetails(bool customRoundOff = false)
	{
		foreach (var item in _cart)
		{
			if (item.Quantity == 0)
				_cart.Remove(item);

			item.BaseTotal = item.Rate * item.Quantity;
			item.DiscountAmount = item.BaseTotal * (item.DiscountPercent / 100);
			item.AfterDiscount = item.BaseTotal - item.DiscountAmount;
			item.CGSTAmount = item.AfterDiscount * (item.CGSTPercent / 100);
			item.SGSTAmount = item.AfterDiscount * (item.SGSTPercent / 100);
			item.IGSTAmount = item.AfterDiscount * (item.IGSTPercent / 100);
			item.TotalTaxAmount = item.CGSTAmount + item.SGSTAmount + item.IGSTAmount;
			item.Total = item.InclusiveTax ? item.AfterDiscount : item.AfterDiscount + item.TotalTaxAmount;
			item.NetRate = item.Total / item.Quantity * (1 + (_purchase.OtherChargesPercent * 100)) * (1 - (_purchase.CashDiscountPercent / 100));
		}

		_purchase.ItemsTotalAmount = _cart.Sum(x => x.Total);

		_itemBaseTotal = _cart.Sum(x => x.BaseTotal);
		_itemDiscountTotal = _cart.Sum(x => x.DiscountAmount);
		_itemAfterDiscountTotal = _cart.Sum(x => x.AfterDiscount);
		_itemTaxTotal = _cart.Sum(x => x.TotalTaxAmount);
		_itemAfterTaxTotal = _cart.Sum(x => x.Total);

		_purchase.OtherChargesAmount = _itemAfterTaxTotal * (_purchase.OtherChargesPercent) / 100;
		var totalAfterOtherCharges = _itemAfterTaxTotal + (_purchase.OtherChargesAmount);

		_purchase.CashDiscountAmount = totalAfterOtherCharges * (_purchase.CashDiscountPercent) / 100;
		var totalAfterCashDiscount = totalAfterOtherCharges - (_purchase.CashDiscountAmount);

		if (!customRoundOff)
			_purchase.RoundOffAmount = Math.Round(totalAfterCashDiscount) - totalAfterCashDiscount;

		_purchase.TotalAmount = totalAfterCashDiscount + _purchase.RoundOffAmount;

		_purchase.CompanyId = _selectedCompany.Id;
		_purchase.PartyId = _selectedParty.Id;
		_purchase.UserId = _user.Id;

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_purchase.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_purchase.FinancialYearId = _selectedFinancialYear.Id;
		else
		{
			await ShowToast("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", "error");
			_purchase.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_purchase.TransactionDateTime);
			_purchase.FinancialYearId = _selectedFinancialYear.Id;
		}
		#endregion

		if (_autoGenerateTransactionNo)
			_purchase.TransactionNo = await GenerateCodes.GeneratePurchaseTransactionNo(_purchase);
	}

	private async Task SavePurchaseFile(bool customRoundOff = false)
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails(customRoundOff);

			await DataStorageService.LocalSaveAsync(StorageFileNames.PurchaseDataFileName, System.Text.Json.JsonSerializer.Serialize(_purchase));
			await DataStorageService.LocalSaveAsync(StorageFileNames.PurchaseCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cart));
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Saving Purchase Data", ex.Message, "error");
		}
		finally
		{
			if (_sfCartGrid is not null)
				await _sfCartGrid?.Refresh();

			StateHasChanged();

			_isProcessing = false;
		}
	}

	private async Task<bool> ValidateForm()
	{
		if (_cart.Count == 0)
		{
			await ShowToast("Cart is Empty", "Please add at least one item to the cart before saving the transaction.", "error");
			return false;
		}

		if (_selectedCompany is null || _purchase.CompanyId <= 0)
		{
			await ShowToast("Company Not Selected", "Please select a company for the purchase transaction.", "error");
			return false;
		}

		if (_selectedParty is null || _purchase.PartyId <= 0)
		{
			await ShowToast("Party Not Selected", "Please select a party ledger for the purchase transaction.", "error");
			return false;
		}

		if (string.IsNullOrEmpty(_purchase.TransactionNo) || string.IsNullOrWhiteSpace(_purchase.TransactionNo))
		{
			await ShowToast("Transaction Number Missing", "Please enter a transaction number for the purchase.", "error");
			return false;
		}

		if (_purchase.TransactionDateTime == default)
		{
			await ShowToast("Transaction Date Missing", "Please select a valid transaction date for the purchase.", "error");
			return false;
		}

		if (_selectedFinancialYear is null || _purchase.FinancialYearId <= 0)
		{
			await ShowToast("Financial Year Not Found", "The transaction date does not fall within any financial year. Please check the date and try again.", "error");
			return false;
		}

		if (_selectedFinancialYear.Locked)
		{
			await ShowToast("Financial Year Locked", "The financial year for the selected transaction date is locked. Please select a different date.", "error");
			return false;
		}

		if (_selectedFinancialYear.Status == false)
		{
			await ShowToast("Financial Year Inactive", "The financial year for the selected transaction date is inactive. Please select a different date.", "error");
			return false;
		}

		if (_purchase.TotalAmount <= 0)
		{
			await ShowToast("Invalid Total Amount", "The total amount of the purchase transaction must be greater than zero.", "error");
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

			await SavePurchaseFile(true);

			if (!await ValidateForm())
			{
				_isProcessing = false;
				return;
			}

			_purchase.Status = true;
			var currentDateTime = await CommonData.LoadCurrentDateTime();
			_purchase.TransactionDateTime = DateOnly.FromDateTime(_purchase.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
			_purchase.LastModifiedAt = currentDateTime;
			_purchase.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			_purchase.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			_purchase.UserId = _user.Id;
			_purchase.LastModifiedBy = _user.Id;

			_purchase.Id = await PurchaseData.SavePurchaseTransaction(_purchase, _cart);
			await DeleteLocalFiles();
			NavigationManager.NavigateTo(NavigationManager.Uri, true);

			await ShowToast("Save Transaction", "Transaction saved successfully!", "success");
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Saving Transaction", ex.Message, "error");
		}
		finally
		{
			_isProcessing = false;
		}
	}

	private async Task DeleteLocalFiles()
	{
		await DataStorageService.LocalRemove(StorageFileNames.PurchaseDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.PurchaseCartDataFileName);
	}
	#endregion

	private async Task ShowToast(string title, string message, string type)
	{
		VibrationService.VibrateWithTime(200);

		if (type == "error")
		{
			_errorTitle = title;
			_errorMessage = message;
			await _sfErrorToast.ShowAsync(new()
			{
				Title = _errorTitle,
				Content = _errorMessage
			});
		}

		else if (type == "success")
		{
			_successTitle = title;
			_successMessage = message;
			await _sfSuccessToast.ShowAsync(new()
			{
				Title = _successTitle,
				Content = _successMessage
			});
		}
	}
}