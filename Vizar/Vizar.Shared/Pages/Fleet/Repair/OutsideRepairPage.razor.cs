using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Repair;
using VizarLibrary.Data.Operations;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Repair;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Repair;

public partial class OutsideRepairPage : IAsyncDisposable
{
	private HotKeysContext _hotKeysContext;

	[Parameter] public int? Id { get; set; }

	private UserModel _user;

	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _autoGenerateTransactionNo = false;

	private CompanyModel _selectedCompany = new();
	private LedgerModel _selectedVendor = new();
	private VehicleModel _selectedVehicle = new();
	private FinancialYearModel _selectedFinancialYear = new();
	private OutsideRepairItemCartModel _selectedCart = new();
	private OutsideRepairModel _outsideRepair = new();

	private List<CompanyModel> _companies = [];
	private List<LedgerModel> _vendors = [];
	private List<VehicleModel> _vehicles = [];
	private List<OutsideRepairItemCartModel> _cart = [];

	private SfTextBox _sfJobTextBox;
	private SfGrid<OutsideRepairItemCartModel> _sfCartGrid;

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
			.Add(ModCode.Ctrl, Code.Enter, AddJobToCart, "Add job to cart", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, () => _sfJobTextBox.FocusAsync(), "Focus on job input", Exclude.None)
			.Add(ModCode.Ctrl, Code.S, SaveTransaction, "Save the transaction", Exclude.None)
			.Add(ModCode.Alt, Code.P, DownloadPdfInvoice, "Download PDF invoice", Exclude.None)
			.Add(ModCode.Alt, Code.E, DownloadExcelInvoice, "Download Excel invoice", Exclude.None)
			.Add(ModCode.Ctrl, Code.H, NavigateToTransactionHistoryPage, "Open transaction history", Exclude.None)
			.Add(ModCode.Ctrl, Code.I, NavigateToItemReport, "Open item report", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Go to dashboard", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
			.Add(Code.Delete, RemoveSelectedCartJob, "Delete selected cart job", Exclude.None)
			.Add(Code.Insert, EditSelectedCartJob, "Edit selected cart job", Exclude.None);

		await LoadCompanies();
		await LoadLedgers();
		await LoadVehicles();
		await LoadExistingTransaction();
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

	private async Task LoadLedgers()
	{
		try
		{
			_vendors = await CommonData.LoadTableDataByStatus<LedgerModel>(TableNames.Ledger);
			_vendors = [.. _vendors.OrderBy(s => s.Name)];
			_vendors.Add(new()
			{
				Id = 0,
				Name = "Create New Vendor Ledger..."
			});

			_selectedVendor = _vendors.FirstOrDefault();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("An Error Occurred While Loading Vendors", ex.Message, ToastType.Error);
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
				_outsideRepair = await CommonData.LoadTableDataById<OutsideRepairModel>(TableNames.OutsideRepair, Id.Value);
				if (_outsideRepair is null)
				{
					await _toastNotification.ShowAsync("Transaction Not Found", "The requested transaction could not be found.", ToastType.Error);
					NavigationManager.NavigateTo(PageRouteNames.OutsideRepair, true);
				}
			}

			else if (await DataStorageService.LocalExists(StorageFileNames.OutsideRepairDataFileName))
				_outsideRepair = System.Text.Json.JsonSerializer.Deserialize<OutsideRepairModel>(await DataStorageService.LocalGetAsync(StorageFileNames.OutsideRepairDataFileName));

			else
			{
				_outsideRepair = new()
				{
					Id = 0,
					TransactionNo = string.Empty,
					CompanyId = _selectedCompany.Id,
					VendorId = _selectedVendor?.Id ?? 0,
					VehicleId = _selectedVehicle?.Id ?? 0,
					CurrentHour = null,
					CurrentKM = null,
					ApprovedBy = null,
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

			if (_outsideRepair.CompanyId > 0)
				_selectedCompany = _companies.FirstOrDefault(s => s.Id == _outsideRepair.CompanyId);
			else
			{
				_selectedCompany = _companies.FirstOrDefault();
				_outsideRepair.CompanyId = _selectedCompany.Id;
			}

			if (_outsideRepair.VendorId > 0)
				_selectedVendor = _vendors.FirstOrDefault(s => s.Id == _outsideRepair.VendorId);
			else
			{
				_selectedVendor = _vendors.FirstOrDefault();
				_outsideRepair.VendorId = _selectedVendor?.Id ?? 0;
			}

			if (_outsideRepair.VehicleId > 0)
				_selectedVehicle = _vehicles.FirstOrDefault(s => s.Id == _outsideRepair.VehicleId);
			else
			{
				_selectedVehicle = _vehicles.FirstOrDefault();
				_outsideRepair.VehicleId = _selectedVehicle?.Id ?? 0;
			}

			_selectedFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _outsideRepair.FinancialYearId);
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

	private async Task LoadExistingCart()
	{
		try
		{
			_cart.Clear();

			if (_outsideRepair.Id > 0)
			{
				var existingCart = await CommonData.LoadTableDataByMasterId<OutsideRepairDetailModel>(TableNames.OutsideRepairDetail, _outsideRepair.Id);

				foreach (var item in existingCart)
				{
					_cart.Add(new()
					{
						Job = item.Job,
						Quantity = item.Quantity,
						Rate = item.Rate,
						Total = item.Total,
						Remarks = item.Remarks
					});
				}
			}

			else if (await DataStorageService.LocalExists(StorageFileNames.OutsideRepairCartDataFileName))
				_cart = System.Text.Json.JsonSerializer.Deserialize<List<OutsideRepairItemCartModel>>(await DataStorageService.LocalGetAsync(StorageFileNames.OutsideRepairCartDataFileName));
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
		_outsideRepair.CompanyId = _selectedCompany.Id;

		await SaveTransactionFile();
	}

	private async Task OnVendorChanged(ChangeEventArgs<LedgerModel, LedgerModel> args)
	{
		if (args.Value is null)
			return;

		else if (args.Value.Id == 0)
		{
			if (FormFactor.GetFormFactor() == "Web")
				await JSRuntime.InvokeVoidAsync("open", PageRouteNames.AdminLedger, "_blank");
			else
				NavigationManager.NavigateTo(PageRouteNames.AdminLedger);

			return;
		}

		_selectedVendor = args.Value;
		_outsideRepair.VendorId = _selectedVendor.Id;

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
		_outsideRepair.VehicleId = _selectedVehicle.Id;

		await SaveTransactionFile();
	}

	private async Task OnTransactionDateChanged(Syncfusion.Blazor.Calendars.ChangedEventArgs<DateTime> args)
	{
		_outsideRepair.TransactionDateTime = args.Value;
		await SaveTransactionFile();
	}

	private async Task OnAutoGenerateTransactionNoChecked(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
	{
		_autoGenerateTransactionNo = args.Checked;
		await SaveTransactionFile();
	}
	#endregion

	#region Cart
	private void OnJobQuantityChanged(ChangeEventArgs<decimal> args)
	{
		_selectedCart.Quantity = args.Value;
		UpdateSelectedJobFinancialDetails();
	}

	private void OnJobRateChanged(ChangeEventArgs<decimal> args)
	{
		_selectedCart.Rate = args.Value;
		UpdateSelectedJobFinancialDetails();
	}

	private void UpdateSelectedJobFinancialDetails()
	{
		if (_selectedCart.Quantity <= 0)
			_selectedCart.Quantity = 1;

		_selectedCart.Total = _selectedCart.Rate * _selectedCart.Quantity;

		StateHasChanged();
	}

	private async Task AddJobToCart()
	{
		if (string.IsNullOrWhiteSpace(_selectedCart.Job) || _selectedCart.Quantity <= 0 || _selectedCart.Rate < 0 || _selectedCart.Total < 0)
		{
			await _toastNotification.ShowAsync("Invalid Job Details", "Please ensure all job details are correctly filled before adding to the cart.", ToastType.Error);
			return;
		}

		UpdateSelectedJobFinancialDetails();

		_cart.Add(new()
		{
			Job = _selectedCart.Job,
			Quantity = _selectedCart.Quantity,
			Rate = _selectedCart.Rate,
			Total = _selectedCart.Total,
			Remarks = _selectedCart.Remarks
		});

		_selectedCart = new();

		await _sfJobTextBox.FocusAsync();
		await SaveTransactionFile();
	}

	private async Task EditSelectedCartJob()
	{
		if (_sfCartGrid is null || _sfCartGrid.SelectedRecords is null || _sfCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartJob = _sfCartGrid.SelectedRecords.First();
		await EditCartJob(selectedCartJob);
	}

	private async Task EditCartJob(OutsideRepairItemCartModel cartJob)
	{
		_selectedCart = new()
		{
			Job = cartJob.Job,
			Quantity = cartJob.Quantity,
			Rate = cartJob.Rate,
			Total = cartJob.Total,
			Remarks = cartJob.Remarks
		};

		await _sfJobTextBox.FocusAsync();
		UpdateSelectedJobFinancialDetails();
		await RemoveJobFromCart(cartJob);
	}

	private async Task RemoveSelectedCartJob()
	{
		if (_sfCartGrid is null || _sfCartGrid.SelectedRecords is null || _sfCartGrid.SelectedRecords.Count == 0)
			return;

		var selectedCartJob = _sfCartGrid.SelectedRecords.First();
		await RemoveJobFromCart(selectedCartJob);
	}

	private async Task RemoveJobFromCart(OutsideRepairItemCartModel cartJob)
	{
		_cart.Remove(cartJob);
		await SaveTransactionFile();
	}
	#endregion

	#region Saving
	private async Task UpdateFinancialDetails()
	{
		foreach (var job in _cart)
		{
			if (job.Quantity == 0)
				_cart.Remove(job);

			job.Total = job.Rate * job.Quantity;

			job.Remarks = job.Remarks?.Trim();
			if (string.IsNullOrWhiteSpace(job.Remarks))
				job.Remarks = null;

			job.Job = job.Job?.Trim();
		}

		_outsideRepair.TotalItems = _cart.Count;
		_outsideRepair.TotalQuantity = _cart.Sum(x => x.Quantity);
		_outsideRepair.TotalAmount = _cart.Sum(x => x.Total);

		_outsideRepair.CompanyId = _selectedCompany.Id;
		_outsideRepair.VendorId = _selectedVendor?.Id ?? 0;
		_outsideRepair.VehicleId = _selectedVehicle?.Id ?? 0;
		_outsideRepair.CreatedBy = _user.Id;

		#region Financial Year
		_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_outsideRepair.TransactionDateTime);
		if (_selectedFinancialYear is not null && !_selectedFinancialYear.Locked)
			_outsideRepair.FinancialYearId = _selectedFinancialYear.Id;
		else
		{
			await _toastNotification.ShowAsync("Invalid Transaction Date", "The selected transaction date does not fall within an active financial year.", ToastType.Error);
			_outsideRepair.TransactionDateTime = await CommonData.LoadCurrentDateTime();
			_selectedFinancialYear = await FinancialYearData.LoadFinancialYearByDateTime(_outsideRepair.TransactionDateTime);
			_outsideRepair.FinancialYearId = _selectedFinancialYear.Id;
		}
		#endregion

		if (Id is null && _autoGenerateTransactionNo)
			_outsideRepair.TransactionNo = await GenerateCodes.GenerateOutsideRepairTransactionNo(_outsideRepair);
	}

	private async Task SaveTransactionFile()
	{
		if (_isProcessing || _isLoading)
			return;

		try
		{
			_isProcessing = true;

			await UpdateFinancialDetails();

			await DataStorageService.LocalSaveAsync(StorageFileNames.OutsideRepairDataFileName, System.Text.Json.JsonSerializer.Serialize(_outsideRepair));
			await DataStorageService.LocalSaveAsync(StorageFileNames.OutsideRepairCartDataFileName, System.Text.Json.JsonSerializer.Serialize(_cart));
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
		if (_selectedCompany is null || _outsideRepair.CompanyId <= 0)
		{
			await _toastNotification.ShowAsync("Company Not Selected", "Please select a company for the transaction.", ToastType.Warning);
			return false;
		}

		if (_selectedVendor is null || _outsideRepair.VendorId <= 0)
		{
			await _toastNotification.ShowAsync("Vendor Not Selected", "Please select a vendor for the transaction.", ToastType.Warning);
			return false;
		}

		if (_selectedVehicle is null || _outsideRepair.VehicleId <= 0)
		{
			await _toastNotification.ShowAsync("Vehicle Not Selected", "Please select a vehicle for the transaction.", ToastType.Warning);
			return false;
		}

		if ((_outsideRepair.CurrentKM is null || _outsideRepair.CurrentKM < 0) && (_outsideRepair.CurrentHour is null || _outsideRepair.CurrentHour < 0))
		{
			await _toastNotification.ShowAsync("Current KM/Hour Missing", "Please enter valid current KM and hour for the vehicle.", ToastType.Warning);
			return false;
		}

		if (string.IsNullOrWhiteSpace(_outsideRepair.TransactionNo))
		{
			await _toastNotification.ShowAsync("Transaction Number Missing", "Please enter a transaction number for the transaction.", ToastType.Warning);
			return false;
		}

		if (_outsideRepair.TransactionDateTime == default)
		{
			await _toastNotification.ShowAsync("Transaction Date Missing", "Please select a valid transaction date for the transaction.", ToastType.Warning);
			return false;
		}

		if (_selectedFinancialYear is null || _outsideRepair.FinancialYearId <= 0)
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

		if (_outsideRepair.TotalItems <= 0)
		{
			await _toastNotification.ShowAsync("No Jobs in Cart", "The transaction must contain at least one job in the cart.", ToastType.Warning);
			return false;
		}

		if (_outsideRepair.TotalQuantity <= 0)
		{
			await _toastNotification.ShowAsync("Invalid Total Quantity", "The total quantity of the transaction must be greater than zero.", ToastType.Error);
			return false;
		}

		if (_outsideRepair.TotalAmount < 0)
		{
			await _toastNotification.ShowAsync("Invalid Total Amount", "The total amount of the transaction must be greater than zero.", ToastType.Error);
			return false;
		}

		if (_cart.Any(job => job.Quantity <= 0))
		{
			await _toastNotification.ShowAsync("Invalid Job Quantity", "One or more jobs in the cart have a quantity less than or equal to zero. Please correct the quantities before saving.", ToastType.Error);
			return false;
		}

		if (_outsideRepair.Id > 0)
		{
			var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, _outsideRepair.FinancialYearId);
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

		_outsideRepair.Remarks = _outsideRepair.Remarks?.Trim();
		if (string.IsNullOrWhiteSpace(_outsideRepair.Remarks))
			_outsideRepair.Remarks = null;

		_outsideRepair.ApprovedBy = _outsideRepair.ApprovedBy?.Trim();
		if (string.IsNullOrWhiteSpace(_outsideRepair.ApprovedBy))
			_outsideRepair.ApprovedBy = null;

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

			_outsideRepair.Status = true;
			var currentDateTime = await CommonData.LoadCurrentDateTime();
			_outsideRepair.TransactionDateTime = DateOnly.FromDateTime(_outsideRepair.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
			_outsideRepair.LastModifiedAt = currentDateTime;
			_outsideRepair.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			_outsideRepair.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
			_outsideRepair.CreatedBy = _user.Id;
			_outsideRepair.LastModifiedBy = _user.Id;

			_outsideRepair.Id = await OutsideRepairData.SaveTransaction(_outsideRepair, _cart);

			var (pdfStream, fileName) = await OutsideRepairInvoiceExport.ExportInvoice(_outsideRepair.Id, InvoiceExportType.PDF);
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
		await DataStorageService.LocalRemove(StorageFileNames.OutsideRepairDataFileName);
		await DataStorageService.LocalRemove(StorageFileNames.OutsideRepairCartDataFileName);
	}
	#endregion

	#region Utilities
	private async Task ResetPage()
	{
		await DeleteLocalFiles();
		NavigationManager.NavigateTo(PageRouteNames.OutsideRepair, true);
	}

	private async Task NavigateToTransactionHistoryPage()
	{
		if (FormFactor.GetFormFactor() == "Web")
			await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportOutsideRepair, "_blank");
		else
			NavigationManager.NavigateTo(PageRouteNames.ReportOutsideRepair);
	}

	private async Task NavigateToItemReport()
	{
		if (FormFactor.GetFormFactor() == "Web")
			await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportOutsideRepairItem, "_blank");
		else
			NavigationManager.NavigateTo(PageRouteNames.ReportOutsideRepairItem);
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

			var (pdfStream, fileName) = await OutsideRepairInvoiceExport.ExportInvoice(_outsideRepair.Id, InvoiceExportType.PDF);
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

			var (excelStream, fileName) = await OutsideRepairInvoiceExport.ExportInvoice(_outsideRepair.Id, InvoiceExportType.Excel);
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