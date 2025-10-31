using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;

using Vizar.Shared.Services;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory;

namespace Vizar.Shared.Pages.Reports.Inventory;

public partial class PurchaseReport
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
	private List<PurchaseOverviewModel> _purchaseOverviews = [];

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
		await _sfPurchaseGrid.ExportToExcelAsync();
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
			if (FormFactor.GetFormFactor() == "Web")
				await JSRuntime.InvokeVoidAsync("open", $"/inventory/purchase?id={purchaseId}", "_blank");
			else
				NavigationManager.NavigateTo($"/inventory/purchase?id={purchaseId}");
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
	#endregion

	#region Toggle View
	private async Task ToggleDetailsView()
	{
		_showAllColumns = !_showAllColumns;
		StateHasChanged();

		// Refresh grid to apply column changes
		if (_sfPurchaseGrid is not null)
			await _sfPurchaseGrid.Refresh();
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