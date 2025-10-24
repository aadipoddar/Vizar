using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;

using Vizar.Shared.Services;

using VizarLibrary.Data.Accounts;
using VizarLibrary.Data.Common;
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

	private LedgerModel _mainCompanyLedger = new();
	private LedgerModel _selectedParty = new();
	private PurchaseModel _purchase = new();

	private List<LedgerModel> _parties = [];
	private List<ItemModel> _items = [];
	private List<TaxModel> _taxes = [];
	private List<PurchaseItemCartModel> _cart = [];

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
	}

	private async Task LoadData()
	{
		await LoadLedgers();
		await LoadPurchase();
		await LoadItems();
		await LoadExistingCart();
		await SavePurchaseFile();
		StateHasChanged();
	}

	private async Task LoadLedgers()
	{
		try
		{
			_parties = await CommonData.LoadTableDataByStatus<LedgerModel>(TableNames.Ledger);
			_parties = [.. _parties.OrderBy(s => s.Name)];

			var mainCompanyLedgerId = await SettingsData.LoadSettingsByKey(SettingsKeys.CompanyLedgerId);
			_mainCompanyLedger = _parties.FirstOrDefault(s => s.Id.ToString() == mainCompanyLedgerId.Value) ?? throw new Exception("Main Company Ledger Not Found");

			_parties.RemoveAll(s => s.Id == _mainCompanyLedger.Id);
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Loading Ledgers", ex.Message, "error");
		}
	}

	private async Task LoadPurchase()
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
					TransactionDateTime = await CommonData.LoadCurrentDateTime(),
					FinancialYearId = (await FinancialYearData.LoadFinancialYearByDateTime(await CommonData.LoadCurrentDateTime())).Id,
					PartyId = _parties.FirstOrDefault().Id,
					UserId = _user.Id,
					ItemsTotalAmount = 0,
					CashDiscountPercent = null,
					CashDiscountAmount = null,
					OtherChargesPercent = null,
					OtherChargesAmount = null,
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

				await DataStorageService.LocalRemove(StorageFileNames.PurchaseDataFileName);
				await DataStorageService.LocalRemove(StorageFileNames.PurchaseCartDataFileName);
			}


			if (_purchase.PartyId > 0)
				_selectedParty = _parties.FirstOrDefault(s => s.Id == _purchase.PartyId);
			else
			{
				_selectedParty = _parties.FirstOrDefault();
				_purchase.PartyId = _selectedParty.Id;
			}
		}
		catch (Exception ex)
		{
			await ShowToast("An Error Occurred While Loading Purchase Data", ex.Message, "error");
		}
	}

	private async Task LoadItems()
	{
		try
		{
			_items = await ItemData.LoadItemByPartyPurchaseDateTime(_purchase.PartyId, _purchase.TransactionDateTime);
			_taxes = await CommonData.LoadTableDataByStatus<TaxModel>(TableNames.Tax);

			_items = [.. _items.OrderBy(s => s.Name)];
			_taxes = [.. _taxes.OrderBy(s => s.Name)];
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
		}
	}
	#endregion

	#region Change Events
	private async Task OnPartyChanged(ChangeEventArgs<LedgerModel, LedgerModel> args)
	{
		if (args.Value is null)
			return;

		_selectedParty = args.Value;
		_purchase.PartyId = _selectedParty.Id;

		await LoadItems();
		await SavePurchaseFile();
	}

	private async Task OnTransactionDateChanged(ChangedEventArgs<DateTime> args)
	{
		_purchase.TransactionDateTime = args.Value;
		await LoadItems();
		await SavePurchaseFile();
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
			item.TaxPercent = _taxes.FirstOrDefault(s => s.Id == item.TaxId)?.GST ?? 0;
			item.TaxAmount = item.AfterDiscount * (item.TaxPercent / 100);
			item.Total = item.AfterDiscount + item.TaxAmount;
			item.NetRate = item.Total / item.Quantity * (1 - (_purchase.CashDiscountPercent ?? 0) / 100) + ((_purchase.OtherChargesPercent ?? 0) / 100);
		}

		_purchase.ItemsTotalAmount = _cart.Sum(x => x.Total);

		#region Cash Discount
		if (_purchase.CashDiscountPercent is null || _purchase.CashDiscountPercent == 0)
		{
			_purchase.CashDiscountPercent = null;
			_purchase.CashDiscountAmount = null;
		}
		else
			_purchase.CashDiscountAmount = _purchase.ItemsTotalAmount * (_purchase.CashDiscountPercent ?? 0) / 100;

		var totalAfterCashDiscount = _purchase.ItemsTotalAmount - (_purchase.CashDiscountAmount ?? 0);
		#endregion

		#region Other Charges
		if (_purchase.OtherChargesPercent is null || _purchase.OtherChargesPercent == 0)
		{
			_purchase.OtherChargesPercent = null;
			_purchase.OtherChargesAmount = null;
		}
		else
			_purchase.OtherChargesAmount = totalAfterCashDiscount * (_purchase.OtherChargesPercent ?? 0) / 100;

		var totalAfterOtherCharges = totalAfterCashDiscount + (_purchase.OtherChargesAmount ?? 0);
		#endregion

		if (!customRoundOff)
			_purchase.RoundOffAmount = Math.Round(totalAfterOtherCharges) - totalAfterOtherCharges;

		_purchase.TotalAmount = totalAfterOtherCharges + _purchase.RoundOffAmount;

		_purchase.UserId = _user.Id;

		#region Financial Year
		var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(_purchase.TransactionDateTime);
		if (financialYear is not null && !financialYear.Locked)
			_purchase.FinancialYearId = financialYear.Id;
		else
		{
			await ShowToast("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", "error");
			_purchase.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			financialYear = await FinancialYearData.LoadFinancialYearByDateTime(_purchase.TransactionDateTime);
			_purchase.FinancialYearId = financialYear.Id;
		}
		#endregion
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