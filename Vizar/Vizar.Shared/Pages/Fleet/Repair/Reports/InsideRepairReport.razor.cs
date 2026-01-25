using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

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

namespace Vizar.Shared.Pages.Fleet.Repair.Reports;

public partial class InsideRepairReport : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private PeriodicTimer _autoRefreshTimer;
    private CancellationTokenSource _autoRefreshCts;

    private UserModel _user;

    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showAllColumns = false;
    private bool _showSummary = false;
    private bool _showDeleted = false;

    private DateTime _fromDate = DateTime.Now.Date;
    private DateTime _toDate = DateTime.Now.Date;

    private CompanyModel _selectedCompany = new();
    private GarageModel _selectedGarage = new();
    private VehicleModel _selectedVehicle = new();

    private List<CompanyModel> _companies = [];
    private List<GarageModel> _garages = [];
    private List<VehicleModel> _vehicles = [];
    private List<InsideRepairOverviewModel> _transactionOverviews = [];

    private SfGrid<InsideRepairOverviewModel> _sfGrid;

    private string _deleteTransactionNo = string.Empty;
    private int _deleteTransactionId = 0;
    private string _recoverTransactionNo = string.Empty;
    private int _recoverTransactionId = 0;

    private ToastNotification _toastNotification;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

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
            .Add(ModCode.Ctrl, Code.R, LoadTransactionOverviews, "Refresh Data", Exclude.None)
            .Add(Code.F5, LoadTransactionOverviews, "Refresh Data", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export to Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export to PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, NavigateToTransactionPage, "New Transaction", Exclude.None)
            .Add(ModCode.Ctrl, Code.I, NavigateToItemReport, "Open item report", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Go to dashboard", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.O, ViewSelectedCartItem, "Open Selected Transaction", Exclude.None)
            .Add(ModCode.Alt, Code.P, DownloadSelectedCartItemPdfInvoice, "Download Selected Transaction PDF Invoice", Exclude.None)
            .Add(ModCode.Alt, Code.E, DownloadSelectedCartItemExcelInvoice, "Download Selected Transaction Excel Invoice", Exclude.None)
            .Add(Code.Delete, DeleteSelectedCartItem, "Delete Selected Transaction", Exclude.None);

        await LoadDates();
        await LoadCompanies();
        await LoadVehicles();
        await LoadGarages();
        await LoadTransactionOverviews();
        await StartAutoRefresh();
    }

    private async Task LoadDates()
    {
        _fromDate = await CommonData.LoadCurrentDateTime();
        _toDate = _fromDate;
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

    private async Task LoadGarages()
    {
        _garages = await CommonData.LoadTableDataByStatus<GarageModel>(TableNames.Garage);
        _garages.Add(new()
        {
            Id = 0,
            Name = "All Garages"
        });
        _garages = [.. _garages.OrderBy(s => s.Name)];
        _selectedGarage = _garages.FirstOrDefault(_ => _.Id == 0);
    }

    private async Task LoadVehicles()
    {
        _vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(TableNames.Vehicle);
        _vehicles.Add(new()
        {
            Id = 0,
            Code = "All Vehicles"
        });
        _vehicles = [.. _vehicles.OrderBy(s => s.Code)];
        _selectedVehicle = _vehicles.FirstOrDefault(_ => _.Id == 0);
    }

    private async Task LoadTransactionOverviews()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Loading", "Fetching transactions...", ToastType.Info);

            _transactionOverviews = await CommonData.LoadTableDataByDate<InsideRepairOverviewModel>(
                ViewNames.InsideRepairOverview,
                DateOnly.FromDateTime(_fromDate).ToDateTime(TimeOnly.MinValue),
                DateOnly.FromDateTime(_toDate).ToDateTime(TimeOnly.MaxValue));

            if (!_showDeleted)
                _transactionOverviews = [.. _transactionOverviews.Where(_ => _.Status)];

            if (_selectedCompany?.Id > 0)
                _transactionOverviews = [.. _transactionOverviews.Where(_ => _.CompanyId == _selectedCompany.Id)];

            if (_selectedGarage?.Id > 0)
                _transactionOverviews = [.. _transactionOverviews.Where(_ => _.GarageId == _selectedGarage.Id)];

            if (_selectedVehicle?.Id > 0)
                _transactionOverviews = [.. _transactionOverviews.Where(_ => _.VehicleId == _selectedVehicle.Id)];

            _transactionOverviews = [.. _transactionOverviews.OrderBy(_ => _.TransactionDateTime)];

            if (_showSummary)
                _transactionOverviews = [.. _transactionOverviews
                    .GroupBy(t => t.VehicleCode)
                    .Select(g => new InsideRepairOverviewModel
                    {
                        VehicleCode= g.Key,
                        TotalItems = g.Sum(t => t.TotalItems),
                        TotalQuantity = g.Sum(t => t.TotalQuantity),
                        TotalAmount = g.Sum(t => t.TotalAmount)
                    })];
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to load transactions: {ex.Message}", ToastType.Error);
        }
        finally
        {
            if (_sfGrid is not null)
                await _sfGrid.Refresh();
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
        await LoadTransactionOverviews();
    }

    private async Task OnCompanyChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<CompanyModel, CompanyModel> args)
    {
        _selectedCompany = args.Value;
        await LoadTransactionOverviews();
    }

    private async Task OnGarageChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<GarageModel, GarageModel> args)
    {
        _selectedGarage = args.Value;
        await LoadTransactionOverviews();
    }

    private async Task OnVehicleChanged(Syncfusion.Blazor.DropDowns.ChangeEventArgs<VehicleModel, VehicleModel> args)
    {
        _selectedVehicle = args.Value;
        await LoadTransactionOverviews();
    }

    private async Task HandleDatesChanged((DateTime FromDate, DateTime ToDate) dates)
    {
        _fromDate = dates.FromDate;
        _toDate = dates.ToDate;
        await LoadTransactionOverviews();
    }
    #endregion

    #region Exporting
    private async Task ExportExcel()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Generating Excel file...", ToastType.Info);

            DateOnly? dateRangeStart = _fromDate != default ? DateOnly.FromDateTime(_fromDate) : null;
            DateOnly? dateRangeEnd = _toDate != default ? DateOnly.FromDateTime(_toDate) : null;

            var (stream, fileName) = await InsideRepairReportExport.ExportReport(
                    _transactionOverviews,
                    ReportExportType.Excel,
                    dateRangeStart,
                    dateRangeEnd,
                    _showAllColumns,
                    _showSummary,
                    _selectedCompany?.Id > 0 ? _selectedCompany : null,
                    _selectedGarage?.Id > 0 ? _selectedGarage : null,
                    _selectedVehicle?.Id > 0 ? _selectedVehicle : null
                );

            await SaveAndViewService.SaveAndView(fileName, stream);
            await _toastNotification.ShowAsync("Success", "Excel file downloaded successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Excel export failed: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ExportPdf()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Generating PDF file...", ToastType.Info);

            DateOnly? dateRangeStart = _fromDate != default ? DateOnly.FromDateTime(_fromDate) : null;
            DateOnly? dateRangeEnd = _toDate != default ? DateOnly.FromDateTime(_toDate) : null;

            var (stream, fileName) = await InsideRepairReportExport.ExportReport(
                    _transactionOverviews,
                    ReportExportType.PDF,
                    dateRangeStart,
                    dateRangeEnd,
                    _showAllColumns,
                    _showSummary,
                    _selectedCompany?.Id > 0 ? _selectedCompany : null,
                    _selectedGarage?.Id > 0 ? _selectedGarage : null,
                    _selectedVehicle?.Id > 0 ? _selectedVehicle : null
                );

            await SaveAndViewService.SaveAndView(fileName, stream);
            await _toastNotification.ShowAsync("Success", "PDF file downloaded successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"PDF export failed: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Actions
    private async Task ViewSelectedCartItem()
    {
        if (_sfGrid?.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfGrid.SelectedRecords.First();
        await ViewTransaction(selectedCartItem.Id);
    }

    private async Task ViewTransaction(int transactionId)
    {
        try
        {
            if (FormFactor.GetFormFactor() == "Web")
                await JSRuntime.InvokeVoidAsync("open", $"{PageRouteNames.InsideRepair}/{transactionId}", "_blank");
            else
                NavigationManager.NavigateTo($"{PageRouteNames.InsideRepair}/{transactionId}");
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while opening transaction: {ex.Message}", ToastType.Error);
        }
    }

    private async Task DownloadSelectedCartItemPdfInvoice()
    {
        if (_sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfGrid.SelectedRecords.First();
        await DownloadPdfInvoice(selectedCartItem.Id);
    }

    private async Task DownloadSelectedCartItemExcelInvoice()
    {
        if (_sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfGrid.SelectedRecords.First();
        await DownloadExcelInvoice(selectedCartItem.Id);
    }

    private async Task DownloadPdfInvoice(int transactionId)
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Generating PDF invoice...", ToastType.Info);

            var (pdfStream, fileName) = await InsideRepairInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, pdfStream);

            await _toastNotification.ShowAsync("Success", "PDF invoice downloaded successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while generating PDF invoice: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task DownloadExcelInvoice(int transactionId)
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();
            await _toastNotification.ShowAsync("Processing", "Generating Excel invoice...", ToastType.Info);

            var (excelStream, fileName) = await InsideRepairInvoiceExport.ExportInvoice(transactionId, InvoiceExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, excelStream);

            await _toastNotification.ShowAsync("Success", "Excel invoice downloaded successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while generating Excel invoice: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task DeleteSelectedCartItem()
    {
        if (_sfGrid is null || _sfGrid.SelectedRecords is null || _sfGrid.SelectedRecords.Count == 0)
            return;

        var selectedCartItem = _sfGrid.SelectedRecords.First();

        if (!selectedCartItem.Status)
            await ShowRecoverConfirmation(selectedCartItem.Id, selectedCartItem.TransactionNo);
        else
            await ShowDeleteConfirmation(selectedCartItem.Id, selectedCartItem.TransactionNo);
    }

    private async Task ConfirmDelete()
    {
        if (_isProcessing)
            return;

        try
        {
            await _deleteConfirmationDialog.HideAsync();
            _isProcessing = true;
            StateHasChanged();

            if (!_user.Admin)
                throw new UnauthorizedAccessException("You do not have permission to delete this transaction.");

            await _toastNotification.ShowAsync("Processing", "Deleting transaction...", ToastType.Info);

            await DeleteTransaction(_deleteTransactionId);

            await _toastNotification.ShowAsync("Success", $"Transaction {_deleteTransactionNo} has been deleted successfully.", ToastType.Success);

            _deleteTransactionId = 0;
            _deleteTransactionNo = string.Empty;
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while deleting transaction: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
            await LoadTransactionOverviews();
        }
    }

    private async Task DeleteTransaction(int deleteTransactionId)
    {
        var itemIssue = await CommonData.LoadTableDataById<InsideRepairModel>(TableNames.InsideRepair, deleteTransactionId);
        if (itemIssue is null)
        {
            await _toastNotification.ShowAsync("Error", "Transaction not found.", ToastType.Error);
            return;
        }

        itemIssue.Status = false;
        itemIssue.LastModifiedBy = _user.Id;
        itemIssue.LastModifiedAt = await CommonData.LoadCurrentDateTime();
        itemIssue.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();

        await InsideRepairData.DeleteTransaction(itemIssue);
    }

    private async Task ConfirmRecover()
    {
        if (_isProcessing)
            return;

        try
        {
            await _recoverConfirmationDialog.HideAsync();
            _isProcessing = true;
            StateHasChanged();

            if (!_user.Admin)
                throw new UnauthorizedAccessException("You do not have permission to recover this transaction.");

            await _toastNotification.ShowAsync("Processing", "Recovering transaction...", ToastType.Info);

            if (_recoverTransactionId == 0)
            {
                await _toastNotification.ShowAsync("Error", "Invalid transaction selected for recovery.", ToastType.Error);
                return;
            }

            await RecoverTransaction(_recoverTransactionId);

            await _toastNotification.ShowAsync("Success", $"Transaction {_recoverTransactionNo} has been recovered successfully.", ToastType.Success);

            _recoverTransactionId = 0;
            _recoverTransactionNo = string.Empty;
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while recovering transaction: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
            await LoadTransactionOverviews();
        }
    }

    private async Task RecoverTransaction(int recoverTransactionId)
    {
        var itemIssue = await CommonData.LoadTableDataById<InsideRepairModel>(TableNames.InsideRepair, recoverTransactionId);
        if (itemIssue is null)
        {
            await _toastNotification.ShowAsync("Error", "Transaction not found.", ToastType.Error);
            return;
        }

        itemIssue.Status = true;
        itemIssue.LastModifiedBy = _user.Id;
        itemIssue.LastModifiedAt = await CommonData.LoadCurrentDateTime();
        itemIssue.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();

        await InsideRepairData.RecoverTransaction(itemIssue);
    }
    #endregion

    #region Utilities
    private async Task ToggleDetailsView()
    {
        _showAllColumns = !_showAllColumns;
        StateHasChanged();

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }

    private async Task ToggleDeleted()
    {
        _showDeleted = !_showDeleted;
        await LoadTransactionOverviews();
        StateHasChanged();
    }

    private async Task ToggleSummary()
    {
        _showSummary = !_showSummary;
        await LoadTransactionOverviews();
    }

    private async Task NavigateToTransactionPage()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.InsideRepair, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.InsideRepair);
    }

    private async Task NavigateToItemReport()
    {
        if (FormFactor.GetFormFactor() == "Web")
            await JSRuntime.InvokeVoidAsync("open", PageRouteNames.ReportInsideRepairItem, "_blank");
        else
            NavigationManager.NavigateTo(PageRouteNames.ReportInsideRepairItem);
    }

    private void NavigateToDashboard() =>
        NavigationManager.NavigateTo(PageRouteNames.Dashboard);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

    private async Task Logout() =>
        await AuthenticationService.Logout(DataStorageService, NavigationManager, VibrationService);

    private async Task ShowDeleteConfirmation(int id, string transactionNo)
    {
        _deleteTransactionId = id;
        _deleteTransactionNo = transactionNo;
        StateHasChanged();
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteTransactionId = 0;
        _deleteTransactionNo = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ShowRecoverConfirmation(int id, string transactionNo)
    {
        _recoverTransactionId = id;
        _recoverTransactionNo = transactionNo;
        StateHasChanged();
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverTransactionId = 0;
        _recoverTransactionNo = string.Empty;
        await _recoverConfirmationDialog.HideAsync();
    }

    private async Task StartAutoRefresh()
    {
        var timerSetting = await SettingsData.LoadSettingsByKey(SettingsKeys.AutoRefreshReportTimer);
        var refreshMinutes = int.TryParse(timerSetting?.Value, out var minutes) ? minutes : 5;

        _autoRefreshCts = new CancellationTokenSource();
        _autoRefreshTimer = new PeriodicTimer(TimeSpan.FromMinutes(refreshMinutes));
        _ = AutoRefreshLoop(_autoRefreshCts.Token);
    }

    private async Task AutoRefreshLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (await _autoRefreshTimer.WaitForNextTickAsync(cancellationToken))
                await LoadTransactionOverviews();
        }
        catch (OperationCanceledException)
        {
            // Timer was cancelled, expected on dispose
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_autoRefreshCts is not null)
        {
            await _autoRefreshCts.CancelAsync();
            _autoRefreshCts.Dispose();
        }

        _autoRefreshTimer?.Dispose();

        if (_hotKeysContext is not null)
            await _hotKeysContext.DisposeAsync();

        GC.SuppressFinalize(this);
    }
    #endregion
}