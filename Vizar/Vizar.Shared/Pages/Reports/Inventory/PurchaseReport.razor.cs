using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;

using Vizar.Shared.Services;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Purchase;
using VizarLibrary.Models.Accounts;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory;

namespace Vizar.Shared.Pages.Reports.Inventory;

public partial class PurchaseReport
{
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showAllColumns = false;
	private bool _showPurchaseReturns = false;

	private DateTime _fromDate = DateTime.Now.Date;
	private DateTime _toDate = DateTime.Now.Date;

	private CompanyModel _selectedCompany = new();
	private LedgerModel _selectedParty = new();

	private List<CompanyModel> _companies = [];
	private List<LedgerModel> _parties = [];
	private List<PurchaseOverviewModel> _purchaseOverviews = [];
	private List<PurchaseReturnOverviewModel> _purchaseReturnOverviews = [];

	private SfGrid<PurchaseOverviewModel> _sfPurchaseGrid;

	private string _errorTitle = string.Empty;
	private string _errorMessage = string.Empty;

	private SfToast _sfErrorToast;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, UserRoles.Inventory);
		await LoadData();
		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		await LoadCompanies();
		await LoadParties();
		await LoadPurchaseOverviews();
	}

	private async Task LoadCompanies()
	{
		_companies = await CommonData.LoadTableDataByStatus<CompanyModel>(TableNames.Company);
		_companies.Add(new()
		{
			Id = 0,
			Name = "All Companies"
		});
		_companies = [.. _companies.OrderBy(s => s.Name)];
		_selectedCompany = _companies.FirstOrDefault(_ => _.Id == 0);
	}

	private async Task LoadParties()
	{
		_parties = await CommonData.LoadTableDataByStatus<LedgerModel>(TableNames.Ledger);
		_parties.Add(new()
		{
			Id = 0,
			Name = "All Parties"
		});
		_parties = [.. _parties.OrderBy(s => s.Name)];
		_selectedParty = _parties.FirstOrDefault(_ => _.Id == 0);
	}

	private async Task LoadPurchaseOverviews()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;

			_purchaseOverviews = await PurchaseData.LoadPurchaseOverviewByDate(
			DateOnly.FromDateTime(_fromDate).ToDateTime(TimeOnly.MinValue),
			DateOnly.FromDateTime(_toDate).ToDateTime(TimeOnly.MaxValue));

			if (_selectedCompany?.Id > 0)
				_purchaseOverviews = [.. _purchaseOverviews.Where(_ => _.CompanyId == _selectedCompany.Id)];

			if (_selectedParty?.Id > 0)
				_purchaseOverviews = [.. _purchaseOverviews.Where(_ => _.PartyId == _selectedParty.Id)];

			_purchaseOverviews = [.. _purchaseOverviews.OrderBy(_ => _.TransactionDateTime)];

			if (_showPurchaseReturns)
				await LoadPurchaseReturnOverviews();
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while loading purchase overviews: {ex.Message}", "error");
		}
		finally
		{
			if (_sfPurchaseGrid is not null)
				await _sfPurchaseGrid.Refresh();
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task LoadPurchaseReturnOverviews()
	{
		_purchaseReturnOverviews = await PurchaseReturnData.LoadPurchaseReturnOverviewByDate(
			DateOnly.FromDateTime(_fromDate).ToDateTime(TimeOnly.MinValue),
			DateOnly.FromDateTime(_toDate).ToDateTime(TimeOnly.MaxValue));

		if (_selectedCompany?.Id > 0)
			_purchaseReturnOverviews = [.. _purchaseReturnOverviews.Where(_ => _.CompanyId == _selectedCompany.Id)];

		if (_selectedParty?.Id > 0)
			_purchaseReturnOverviews = [.. _purchaseReturnOverviews.Where(_ => _.PartyId == _selectedParty.Id)];

		_purchaseReturnOverviews = [.. _purchaseReturnOverviews.OrderBy(_ => _.TransactionDateTime)];

		MergePurchaseAndReturns();
	}

	private void MergePurchaseAndReturns()
	{
		_purchaseOverviews.AddRange(_purchaseReturnOverviews.Select(pr => new PurchaseOverviewModel
		{
			Id = pr.Id * -1, // Negative ID to differentiate returns
			CompanyId = pr.CompanyId,
			CompanyName = pr.CompanyName,
			PartyId = pr.PartyId,
			PartyName = pr.PartyName,
			TransactionDateTime = pr.TransactionDateTime,
			CashDiscountAmount = -pr.CashDiscountAmount,
			OtherChargesAmount = -pr.OtherChargesAmount,
			RoundOffAmount = -pr.RoundOffAmount,
			TotalAmount = -pr.TotalAmount,
			AfterDiscount = -pr.AfterDiscount,
			BaseTotal = -pr.BaseTotal,
			CashDiscountPercent = pr.CashDiscountPercent,
			CGSTAmount = -pr.CGSTAmount,
			CGSTPercent = pr.CGSTPercent,
			CreatedAt = pr.CreatedAt,
			CreatedBy = pr.CreatedBy,
			CreatedByName = pr.CreatedByName,
			CreatedFromPlatform = pr.CreatedFromPlatform,
			DiscountAmount = -pr.DiscountAmount,
			DiscountPercent = pr.DiscountPercent,
			DocumentUrl = pr.DocumentUrl,
			FinancialYear = pr.FinancialYear,
			FinancialYearId = pr.FinancialYearId,
			IGSTAmount = -pr.IGSTAmount,
			IGSTPercent = pr.IGSTPercent,
			Remarks = pr.Remarks,
			LastModifiedAt = pr.LastModifiedAt,
			LastModifiedBy = pr.LastModifiedBy,
			LastModifiedByUserName = pr.LastModifiedByUserName,
			LastModifiedFromPlatform = pr.LastModifiedFromPlatform,
			SGSTAmount = -pr.SGSTAmount,
			SGSTPercent = pr.SGSTPercent,
			TotalAfterCashDiscount = -pr.TotalAfterCashDiscount,
			TotalAfterOtherCharges = -pr.TotalAfterOtherCharges,
			TotalAfterTax = -pr.TotalAfterTax,
			TotalItems = pr.TotalItems,
			TotalQuantity = -pr.TotalQuantity,
			TotalTaxAmount = -pr.TotalTaxAmount,
			TransactionNo = pr.TransactionNo,
			OtherChargesPercent = pr.OtherChargesPercent
		}));

		_purchaseOverviews = [.. _purchaseOverviews.OrderBy(_ => _.TransactionDateTime)];
	}
	#endregion

	#region Changed Events
	private async Task OnDateRangeChanged(Syncfusion.Blazor.Calendars.RangePickerEventArgs<DateTime> args)
	{
		_fromDate = args.StartDate;
		_toDate = args.EndDate;
		await LoadPurchaseOverviews();
	}

	private async Task OnCompanyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<CompanyModel, CompanyModel> args)
	{
		_selectedCompany = args.Value;
		await LoadPurchaseOverviews();
	}

	private async Task OnPartyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<LedgerModel, LedgerModel> args)
	{
		_selectedParty = args.Value;
		await LoadPurchaseOverviews();
	}
	#endregion

	#region Exporting
	private async Task ExportExcel(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			// Convert DateTime to DateOnly for Excel export
			DateOnly? dateRangeStart = _fromDate != default ? DateOnly.FromDateTime(_fromDate) : null;
			DateOnly? dateRangeEnd = _toDate != default ? DateOnly.FromDateTime(_toDate) : null;

			// Call the Excel export utility
			var stream = await Task.Run(() =>
				PurchaseReportExcelExport.ExportPurchaseReport(
					_purchaseOverviews,
					dateRangeStart,
					dateRangeEnd,
					_showAllColumns
				)
			);

			// Generate file name with date range
			string fileName = $"PURCHASE_REPORT";
			if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			{
				fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
			}
			fileName += ".xlsx";

			// Save and view the Excel file
			await SaveAndViewService.SaveAndView(fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream);

			await ShowToast("Success", "Purchase report exported to Excel successfully.", "success");
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while exporting to Excel: {ex.Message}", "error");
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ExportPdf(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
	{
		await _sfPurchaseGrid.ExportToPdfAsync();
	}

	private async Task ExportPowerBI(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
	{
		await ShowToast("Info", "Power BI export is not implemented yet.", "error");
	}
	#endregion

	#region Actions
	private async Task ViewPurchase(int purchaseId)
	{
		try
		{
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while opening purchase: {ex.Message}", "error");
		}
	}

	private async Task DownloadInvoice(int purchaseId)
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			// TODO: Implement invoice generation and download logic
			await ShowToast("Info", "Invoice download functionality will be implemented soon.", "error");
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while downloading invoice: {ex.Message}", "error");
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task DownloadOriginalInvoice(string documentUrl)
	{
		if (_isProcessing)
			return;

		try
		{
			if (string.IsNullOrEmpty(documentUrl))
			{
				await ShowToast("Warning", "No original document available for this purchase.", "error");
				return;
			}

			_isProcessing = true;

			var (fileStream, contentType) = await BlobStorageAccess.DownloadFileFromBlobStorage(documentUrl, BlobStorageContainers.purchase);
			var fileName = documentUrl.Split('/').Last();
			await SaveAndViewService.SaveAndView(fileName, contentType, fileStream);
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while downloading original invoice: {ex.Message}", "error");
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ToggleDetailsView()
	{
		_showAllColumns = !_showAllColumns;
		StateHasChanged();

		if (_sfPurchaseGrid is not null)
			await _sfPurchaseGrid.Refresh();
	}

	private async Task TogglePurchaseReturns()
	{
		_showPurchaseReturns = !_showPurchaseReturns;
		await LoadPurchaseOverviews();
		StateHasChanged();
	}
	#endregion

	#region Utilities
	private async Task NavigateToPurchasePage()
	{
		if (FormFactor.GetFormFactor() == "Web")
			await JSRuntime.InvokeVoidAsync("open", "/inventory/purchase", "_blank");
		else
			NavigationManager.NavigateTo("/inventory/purchase");
	}

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
	}
	#endregion
}