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

public partial class PurchaseReturnReport
{
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showAllColumns = false;

	private DateTime _fromDate = DateTime.Now.Date;
	private DateTime _toDate = DateTime.Now.Date;

	private CompanyModel _selectedCompany = new();
	private LedgerModel _selectedParty = new();

	private List<CompanyModel> _companies = [];
	private List<LedgerModel> _parties = [];
	private List<PurchaseReturnOverviewModel> _purchaseReturnOverviews = [];

	private SfGrid<PurchaseReturnOverviewModel> _sfPurchaseReturnGrid;

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
		await LoadPurchaseReturnOverviews();
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

	private async Task LoadPurchaseReturnOverviews()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;

			_purchaseReturnOverviews = await PurchaseReturnData.LoadPurchaseReturnOverviewByDate(
			DateOnly.FromDateTime(_fromDate).ToDateTime(TimeOnly.MinValue),
			DateOnly.FromDateTime(_toDate).ToDateTime(TimeOnly.MaxValue));

			if (_selectedCompany?.Id > 0)
				_purchaseReturnOverviews = [.. _purchaseReturnOverviews.Where(_ => _.CompanyId == _selectedCompany.Id)];

			if (_selectedParty?.Id > 0)
				_purchaseReturnOverviews = [.. _purchaseReturnOverviews.Where(_ => _.PartyId == _selectedParty.Id)];

			_purchaseReturnOverviews = [.. _purchaseReturnOverviews.OrderBy(_ => _.TransactionDateTime)];
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while loading purchase return overviews: {ex.Message}", "error");
		}
		finally
		{
			if (_sfPurchaseReturnGrid is not null)
				await _sfPurchaseReturnGrid.Refresh();
			_isProcessing = false;
			StateHasChanged();
		}
	}
	#endregion

	#region Changed Events
	private async Task OnDateRangeChanged(Syncfusion.Blazor.Calendars.RangePickerEventArgs<DateTime> args)
	{
		_fromDate = args.StartDate;
		_toDate = args.EndDate;
		await LoadPurchaseReturnOverviews();
	}

	private async Task OnCompanyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<CompanyModel, CompanyModel> args)
	{
		_selectedCompany = args.Value;
		await LoadPurchaseReturnOverviews();
	}

	private async Task OnPartyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<LedgerModel, LedgerModel> args)
	{
		_selectedParty = args.Value;
		await LoadPurchaseReturnOverviews();
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
				PurchaseReturnReportExcelExport.ExportPurchaseReturnReport(
					_purchaseReturnOverviews,
					dateRangeStart,
					dateRangeEnd,
					_showAllColumns
				)
			);

			// Generate file name with date range
			string fileName = $"PURCHASE_RETURN_REPORT";
			if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			{
				fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
			}
			fileName += ".xlsx";

			// Save and view the Excel file
			await SaveAndViewService.SaveAndView(fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream);

			await ShowToast("Success", "Purchase return report exported to Excel successfully.", "success");
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
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			// Convert DateTime to DateOnly for PDF export
			DateOnly? dateRangeStart = _fromDate != default ? DateOnly.FromDateTime(_fromDate) : null;
			DateOnly? dateRangeEnd = _toDate != default ? DateOnly.FromDateTime(_toDate) : null;

			// Call the PDF export utility
			var stream = await Task.Run(() =>
				PurchaseReturnReportPdfExport.ExportPurchaseReturnReport(
					_purchaseReturnOverviews,
					dateRangeStart,
					dateRangeEnd,
					_showAllColumns
				)
			);

			// Generate file name with date range
			string fileName = $"PURCHASE_RETURN_REPORT";
			if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			{
				fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
			}
			fileName += ".pdf";

			// Save and view the PDF file
			await SaveAndViewService.SaveAndView(fileName, "application/pdf", stream);

			await ShowToast("Success", "Purchase return report exported to PDF successfully.", "success");
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while exporting to PDF: {ex.Message}", "error");
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ExportPowerBI(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
	{
		await ShowToast("Info", "Power BI export is not implemented yet.", "error");
	}
	#endregion

	#region Actions
	private async Task ViewPurchaseReturn(int purchaseId)
	{
		try
		{
			if (FormFactor.GetFormFactor() == "Web")
				await JSRuntime.InvokeVoidAsync("open", $"/inventory/purchasereturn/{purchaseId}", "_blank");
			else
				NavigationManager.NavigateTo($"/inventory/purchasereturn/{purchaseId}");
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while opening purchase return: {ex.Message}", "error");
		}
	}

	private async Task DownloadInvoice(int purchaseReturnId)
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			// Load purchase return header
			var purchaseReturnHeader = await CommonData.LoadTableDataById<PurchaseReturnModel>(TableNames.PurchaseReturn, purchaseReturnId);
			if (purchaseReturnHeader == null)
			{
				await ShowToast("Error", "Purchase return not found.", "error");
				return;
			}

			// Load purchase return details
			var purchaseReturnDetails = await PurchaseReturnData.LoadPurchaseReturnDetailByPurchaseReturn(purchaseReturnId);
			if (purchaseReturnDetails == null || !purchaseReturnDetails.Any())
			{
				await ShowToast("Error", "No line items found for this purchase return.", "error");
				return;
			}

			// Load company information
			var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, purchaseReturnHeader.CompanyId);
			if (company == null)
			{
				await ShowToast("Error", "Company information not found.", "error");
				return;
			}

			// Load party (supplier) information
			var party = await CommonData.LoadTableDataById<LedgerModel>(TableNames.Ledger, purchaseReturnHeader.PartyId);
			if (party == null)
			{
				await ShowToast("Error", "Party information not found.", "error");
				return;
			}

			// Generate invoice PDF
			var pdfStream = await Task.Run(() =>
				PurchaseReturnInvoicePDFExport.ExportPurchaseReturnInvoice(
					purchaseReturnHeader,
					purchaseReturnDetails,
					company,
					party,
					null, // logo path - uses default
					"PURCHASE RETURN"
				)
			);

			// Generate file name
			string fileName = $"PURCHASE_RETURN_{purchaseReturnHeader.TransactionNo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

			// Save and view the PDF
			await SaveAndViewService.SaveAndView(fileName, "application/pdf", pdfStream);

			await ShowToast("Success", "Purchase return invoice generated successfully.", "success");
		}
		catch (Exception ex)
		{
			await ShowToast("Error", $"An error occurred while generating invoice: {ex.Message}", "error");
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
				await ShowToast("Warning", "No original document available for this purchase return.", "error");
				return;
			}

			_isProcessing = true;

			var (fileStream, contentType) = await BlobStorageAccess.DownloadFileFromBlobStorage(documentUrl, BlobStorageContainers.purchasereturn);
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

		if (_sfPurchaseReturnGrid is not null)
			await _sfPurchaseReturnGrid.Refresh();
	}
	#endregion

	#region Utilities
	private async Task NavigateToPurchaseReturnPage()
	{
		if (FormFactor.GetFormFactor() == "Web")
			await JSRuntime.InvokeVoidAsync("open", "/inventory/purchasereturn", "_blank");
		else
			NavigationManager.NavigateTo("/inventory/purchasereturn");
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