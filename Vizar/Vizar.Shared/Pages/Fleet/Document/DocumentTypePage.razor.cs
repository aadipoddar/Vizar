using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Document;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Document;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Document;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Document;

public partial class DocumentTypePage : IAsyncDisposable
{
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private DocumentTypeModel _documentType = new();

	private List<DocumentTypeModel> _documentTypes = [];

	private SfGrid<DocumentTypeModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteDocumentTypeId = 0;
	private string _deleteDocumentTypeName = string.Empty;

	private int _recoverDocumentTypeId = 0;
	private string _recoverDocumentTypeName = string.Empty;

	private ToastNotification _toastNotification;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, UserRoles.Admin);
		await LoadData();
		_isLoading = false;
		StateHasChanged();
	}

	private async Task LoadData()
	{
		_hotKeysContext = HotKeys.CreateContext()
			.Add(ModCode.Ctrl, Code.S, SaveDocumentType, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

		_documentTypes = await CommonData.LoadTableData<DocumentTypeModel>(TableNames.DocumentType);

		if (!_showDeleted)
			_documentTypes = [.. _documentTypes.Where(l => l.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Actions
	private void OnEditDocumentType(DocumentTypeModel documentType)
	{
		_documentType = new()
		{
			Id = documentType.Id,
			Name = documentType.Name,
			Code = documentType.Code,
			Rate = documentType.Rate,
			Remarks = documentType.Remarks,
			Status = documentType.Status
		};

		StateHasChanged();
	}

	private async Task ShowDeleteConfirmation(int id, string name)
	{
		_deleteDocumentTypeId = id;
		_deleteDocumentTypeName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteDocumentTypeId = 0;
		_deleteDocumentTypeName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ConfirmDelete()
	{
		try
		{
			_isProcessing = true;
			await _deleteConfirmationDialog.HideAsync();

			var documentType = _documentTypes.FirstOrDefault(l => l.Id == _deleteDocumentTypeId);
			if (documentType == null)
			{
				await _toastNotification.ShowAsync("Error", "Document Type not found.", ToastType.Error);
				return;
			}

			documentType.Status = false;
			await DocumentData.InsertDocumentType(documentType);

			await _toastNotification.ShowAsync("Deleted", $"Document Type '{documentType.Name}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.AdminDocumentType, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Document Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteDocumentTypeId = 0;
			_deleteDocumentTypeName = string.Empty;
		}
	}

	private async Task ShowRecoverConfirmation(int id, string name)
	{
		_recoverDocumentTypeId = id;
		_recoverDocumentTypeName = name;
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverDocumentTypeId = 0;
		_recoverDocumentTypeName = string.Empty;
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

			var documentType = _documentTypes.FirstOrDefault(l => l.Id == _recoverDocumentTypeId);
			if (documentType == null)
			{
				await _toastNotification.ShowAsync("Error", "Document Type not found.", ToastType.Error);
				return;
			}

			documentType.Status = true;
			await DocumentData.InsertDocumentType(documentType);

			await _toastNotification.ShowAsync("Recovered", $"Document Type '{documentType.Name}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.AdminDocumentType, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Document Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverDocumentTypeId = 0;
			_recoverDocumentTypeName = string.Empty;
		}
	}
	#endregion

	#region Saving
	private async Task<bool> ValidateForm()
	{
		_documentType.Name = _documentType.Name?.Trim() ?? "";
		_documentType.Name = _documentType.Name?.ToUpper() ?? "";

		_documentType.Remarks = _documentType.Remarks?.Trim() ?? "";
		_documentType.Status = true;


		if (_documentType.Id == 0)
			_documentType.Code = await GenerateCodes.GenerateDocumentTypeCode();

		if (string.IsNullOrWhiteSpace(_documentType.Name))
		{
			await _toastNotification.ShowAsync("Validation", "Document Type name is required. Please enter a valid name.", ToastType.Warning);
			return false;
		}

		if (_documentType.Rate < 0)
		{
			await _toastNotification.ShowAsync("Validation", "Rate cannot be negative. Please enter a valid rate.", ToastType.Warning);
			return false;
		}

		if (string.IsNullOrWhiteSpace(_documentType.Remarks))
			_documentType.Remarks = null;

		if (_documentType.Id > 0)
		{
			var existingDocumentType = _documentTypes.FirstOrDefault(_ => _.Id != _documentType.Id && _.Name.Equals(_documentType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingDocumentType is not null)
			{
				await _toastNotification.ShowAsync("Duplicate", $"Document Type name '{_documentType.Name}' already exists. Please choose a different name.", ToastType.Warning);
				return false;
			}
		}
		else
		{
			var existingDocumentType = _documentTypes.FirstOrDefault(_ => _.Name.Equals(_documentType.Name, StringComparison.OrdinalIgnoreCase));
			if (existingDocumentType is not null)
			{
				await _toastNotification.ShowAsync("Duplicate", $"Document Type name '{_documentType.Name}' already exists. Please choose a different name.", ToastType.Warning);
				return false;
			}
		}

		return true;
	}

	private async Task SaveDocumentType()
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

			await _toastNotification.ShowAsync("Processing", "Please wait while the document type is being saved...", ToastType.Info);

			await DocumentData.InsertDocumentType(_documentType);

			await _toastNotification.ShowAsync("Saved", $"Document Type '{_documentType.Name}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.AdminDocumentType, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to save Document Type: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
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

			var (stream, fileName) = await DocumentTypeExport.ExportMaster(_documentTypes, ReportExportType.Excel);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Success", "Document Type data exported to Excel successfully.", ToastType.Success);
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

			var (stream, fileName) = await DocumentTypeExport.ExportMaster(_documentTypes, ReportExportType.PDF);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Success", "Document Type data exported to PDF successfully.", ToastType.Success);
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
			OnEditDocumentType(selectedRecords[0]);
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
		{
			if (selectedRecords[0].Status)
				await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].Name);
			else
				await ShowRecoverConfirmation(selectedRecords[0].Id, selectedRecords[0].Name);
		}
	}

	private async Task ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.AdminDocumentType, true);

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