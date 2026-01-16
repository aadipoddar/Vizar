using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Document;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Document;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Document;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Document;

public partial class DocumentPage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;

    private UserModel _user;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;
    private bool _isUploadDialogVisible = false;

    private DocumentModel _document = new() { TransactionDateTime = DateTime.Now, RenewalDate = DateTime.Now.AddYears(1) };

    private List<DocumentTypeModel> _documentTypes = [];
    private List<VehicleModel> _vehicles = [];
    private List<DocumentOverviewModel> _documents = [];

    private SfGrid<DocumentOverviewModel> _sfGrid;
    private SfUploader _sfDocumentUploader;
    private DocumentUploadDialog _uploadDocumentDialog;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteDocumentId = 0;
    private string _deleteDocumentName = string.Empty;

    private int _recoverDocumentId = 0;
    private string _recoverDocumentName = string.Empty;

    private ToastNotification _toastNotification;

    #region Load Data
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, UserRoles.Admin);
        await LoadData();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadData()
    {
        _hotKeysContext = HotKeys.CreateContext()
            .Add(ModCode.Ctrl, Code.S, SaveDocument, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.U, UploadDocument, "Upload document", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _documentTypes = await CommonData.LoadTableDataByStatus<DocumentTypeModel>(TableNames.DocumentType);
        _vehicles = await CommonData.LoadTableDataByStatus<VehicleModel>(TableNames.Vehicle);
        _documents = await CommonData.LoadTableData<DocumentOverviewModel>(ViewNames.DocumentOverview);

        if (!_showDeleted)
            _documents = [.. _documents.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Changed Events
    private async Task OnVehicleChanged(ChangeEventArgs<VehicleModel, VehicleModel> args)
    {
        if (args.Value is null || args.Value.Id <= 0)
            return;

        _document.VehicleId = args.Value.Id;
    }

    private async Task OnDocumentTypeChanged(ChangeEventArgs<DocumentTypeModel, DocumentTypeModel> args)
    {
        if (args.Value is null || args.Value.Id <= 0)
            return;

        _document.DocumentTypeId = args.Value.Id;
        _document.Rate = args.Value.Rate;
    }
    #endregion

    #region Actions
    private async Task OnEditDocument(DocumentOverviewModel document)
    {
        _document = await CommonData.LoadTableDataById<DocumentModel>(TableNames.Document, document.Id);
        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteDocumentId = id;
        _deleteDocumentName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteDocumentId = 0;
        _deleteDocumentName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var document = await CommonData.LoadTableDataById<DocumentModel>(TableNames.Document, _deleteDocumentId);
            if (document == null)
            {
                await _toastNotification.ShowAsync("Error", "Document not found.", ToastType.Error);
                return;
            }

            document.Status = false;
            document.LastModifiedBy = _user.Id;
            document.LastModifiedAt = await CommonData.LoadCurrentDateTime();
            document.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            await DocumentData.SaveTransaction(document);

            await _toastNotification.ShowAsync("Deleted", $"Document '{document.TransactionNo}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.Document, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Document: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteDocumentId = 0;
            _deleteDocumentName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverDocumentId = id;
        _recoverDocumentName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverDocumentId = 0;
        _recoverDocumentName = string.Empty;
        await _recoverConfirmationDialog.HideAsync();
    }

    private async Task ToggleDeleted()
    {
        _showDeleted = !_showDeleted;
        await LoadData();
    }

    private async Task ConfirmRecover()
    {
        try
        {
            _isProcessing = true;
            await _recoverConfirmationDialog.HideAsync();

            var document = await CommonData.LoadTableDataById<DocumentModel>(TableNames.Document, _recoverDocumentId);
            if (document == null)
            {
                await _toastNotification.ShowAsync("Error", "Document not found.", ToastType.Error);
                return;
            }

            document.Status = true;
            document.LastModifiedBy = _user.Id;
            document.LastModifiedAt = await CommonData.LoadCurrentDateTime();
            document.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            await DocumentData.SaveTransaction(document);

            await _toastNotification.ShowAsync("Recovered", $"Document '{document.TransactionNo}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.Document, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Document: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverDocumentId = 0;
            _recoverDocumentName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _document.TransactionNo = _document.TransactionNo?.Trim() ?? "";
        _document.TransactionNo = _document.TransactionNo?.ToUpper() ?? "";

        _document.Remarks = _document.Remarks?.Trim() ?? "";
        _document.Status = true;

        var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(_document.TransactionDateTime);
        _document.FinancialYearId = financialYear?.Id ?? 0;

        if (string.IsNullOrWhiteSpace(_document.TransactionNo))
        {
            await _toastNotification.ShowAsync("Validation", "Document number is required. Please enter a valid number.", ToastType.Warning);
            return false;
        }

        if (_document.TransactionDateTime == DateTime.MinValue)
        {
            await _toastNotification.ShowAsync("Validation", "Transaction date is required. Please select a valid date.", ToastType.Warning);
            return false;
        }

        if (financialYear is null || financialYear.Id <= 0 || _document.FinancialYearId <= 0 || !financialYear.Status || financialYear.Locked)
        {
            await _toastNotification.ShowAsync("Validation", "No financial year found for the selected transaction date. Please check the date and try again.", ToastType.Warning);
            return false;
        }

        if (_document.DocumentTypeId <= 0)
        {
            await _toastNotification.ShowAsync("Validation", "Document type is required. Please select a valid document type.", ToastType.Warning);
            return false;
        }

        if (_document.VehicleId <= 0)
        {
            await _toastNotification.ShowAsync("Validation", "Vehicle is required. Please select a valid vehicle.", ToastType.Warning);
            return false;
        }

        if (_document.Rate < 0)
        {
            await _toastNotification.ShowAsync("Validation", "Rate cannot be negative. Please enter a valid rate.", ToastType.Warning);
            return false;
        }

        if (_document.RenewalDate == DateTime.MinValue)
        {
            await _toastNotification.ShowAsync("Validation", "Renewal date is required. Please select a valid date.", ToastType.Warning);
            return false;
        }

        if (_document.RenewalDate <= _document.TransactionDateTime)
        {
            await _toastNotification.ShowAsync("Validation", "Renewal date must be after the transaction date. Please select a valid renewal date.", ToastType.Warning);
            return false;
        }

        if (_document.CurrentKM is null && _document.CurrentHour is null)
        {
            await _toastNotification.ShowAsync("Validation", "Either Current KM or Current Hour must be provided. Please enter at least one value.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_document.Remarks))
            _document.Remarks = null;

        if (_document.Id > 0)
        {
            var existingDocument = _documents.FirstOrDefault(_ => _.Id != _document.Id && _.TransactionNo.Equals(_document.TransactionNo, StringComparison.OrdinalIgnoreCase));
            if (existingDocument is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Document number '{_document.TransactionNo}' already exists. Please choose a different number.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingDocument = _documents.FirstOrDefault(_ => _.TransactionNo.Equals(_document.TransactionNo, StringComparison.OrdinalIgnoreCase));
            if (existingDocument is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Document number '{_document.TransactionNo}' already exists. Please choose a different number.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveDocument()
    {
        if (_isProcessing)
            return;

        try
        {
            _isProcessing = true;
            StateHasChanged();

            if (!await ValidateForm())
            {
                _isProcessing = false;
                return;
            }

            await _toastNotification.ShowAsync("Processing", "Please wait while the category is being saved...", ToastType.Info);

            _document.Status = true;
            var currentDateTime = await CommonData.LoadCurrentDateTime();
            _document.TransactionDateTime = DateOnly.FromDateTime(_document.TransactionDateTime).ToDateTime(new TimeOnly(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second));
            _document.CreatedAt = currentDateTime;
            _document.LastModifiedAt = currentDateTime;
            _document.CreatedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _document.LastModifiedFromPlatform = FormFactor.GetFormFactor() + FormFactor.GetPlatform();
            _document.CreatedBy = _user.Id;
            _document.LastModifiedBy = _user.Id;

            await DocumentData.SaveTransaction(_document);

            await _toastNotification.ShowAsync("Saved", $"Document '{_document.TransactionNo}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.Document, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Document: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }
    #endregion

    #region Uploading Document
    private void UploadDocument()
    {
        _isUploadDialogVisible = true;
        StateHasChanged();
    }

    private async Task OnRemoveFile(RemovingEventArgs args) =>
        await RemoveExistingDocument();

    private async Task RemoveExistingDocument()
    {
        try
        {
            if (string.IsNullOrEmpty(_document.DocumentUrl))
                return;

            var fileName = _document.DocumentUrl.Split('/').Last();
            await BlobStorageAccess.DeleteFileFromBlobStorage(fileName, BlobStorageContainers.document);
            _document.DocumentUrl = null;

            if (_sfDocumentUploader is not null)
                await _sfDocumentUploader.ClearAllAsync();

            await _toastNotification.ShowAsync("Removed", "Document has been removed successfully.", ToastType.Success);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to remove document: {ex.Message}", ToastType.Error);
        }
    }

    private async Task DownloadExistingDocument()
    {
        try
        {
            if (string.IsNullOrEmpty(_document.DocumentUrl))
                return;

            var (stream, contentType) = await BlobStorageAccess.DownloadFileFromBlobStorage(_document.DocumentUrl);
            var fileName = _document.DocumentUrl.Split('/').Last();
            await SaveAndViewService.SaveAndView(fileName, stream);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to download document: {ex.Message}", ToastType.Error);
        }
    }

    private void CloseUploadDialog()
    {
        _isUploadDialogVisible = false;
        StateHasChanged();
    }

    private async Task OnUploaderFileChange(UploadChangeEventArgs args)
    {
        try
        {
            var uploadedFiles = args.Files;

            if (uploadedFiles is not null && uploadedFiles.Count == 1)
            {
                if (!string.IsNullOrEmpty(_document.DocumentUrl))
                    await RemoveExistingDocument();

                await using var file = uploadedFiles[0].File.OpenReadStream(maxAllowedSize: 52428800); // 50 MB
                var fileName = $"{Guid.NewGuid()}_{uploadedFiles[0].File.Name}";
                var fileUrl = await BlobStorageAccess.UploadFileToBlobStorage(file, fileName, BlobStorageContainers.document);
                _document.DocumentUrl = fileUrl;
                await _toastNotification.ShowAsync("Document Uploaded Successfully", "The document has been uploaded and linked to the transaction.", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to upload document: {ex.Message}", ToastType.Error);
        }
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
            await _toastNotification.ShowAsync("Processing", "Please wait while the report is being exported...", ToastType.Info);

            var (stream, fileName) = await DocumentExport.ExportMaster(_documents, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Document data exported to Excel successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while exporting to Excel: {ex.Message}", ToastType.Error);
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
            await _toastNotification.ShowAsync("Processing", "Please wait while the report is being exported...", ToastType.Info);

            var (stream, fileName) = await DocumentExport.ExportMaster(_documents, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Document data exported to PDF successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while exporting to PDF: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Utilities
    private async Task EditSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
            await OnEditDocument(selectedRecords[0]);
    }

    private async Task DeleteSelectedItem()
    {
        var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
        if (selectedRecords.Count > 0)
        {
            if (selectedRecords[0].Status)
                await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].TransactionNo);
            else
                await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].TransactionNo);
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
                await _toastNotification.ShowAsync("Warning", "No original document available for this transaction.", ToastType.Warning);
                return;
            }

            _isProcessing = true;

            var (fileStream, contentType) = await BlobStorageAccess.DownloadFileFromBlobStorage(documentUrl);
            var fileName = documentUrl.Split('/').Last();
            await SaveAndViewService.SaveAndView(fileName, fileStream);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"An error occurred while downloading original invoice: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ResetPage() =>
        NavigationManager.NavigateTo(PageRouteNames.Document, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.FleetDashboard);

    private void NavigateToDashboard() =>
        NavigationManager.NavigateTo(PageRouteNames.Dashboard);

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