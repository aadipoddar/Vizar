using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Service;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Service;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Fleet.Service;

public partial class ServiceSchedulePage : IAsyncDisposable
{
	private HotKeysContext _hotKeysContext;
	private bool _isLoading = true;
	private bool _isProcessing = false;
	private bool _showDeleted = false;

	private ServiceScheduleModel _serviceSchedule = new();

	private List<ServiceScheduleModel> _serviceSchedules = [];
	private List<ServiceTypeModel> _serviceTypes = [];
	private List<VehicleTypeModel> _vehicleTypes = [];

	private SfGrid<ServiceScheduleModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;
	private RecoverConfirmationDialog _recoverConfirmationDialog;

	private int _deleteServiceScheduleId = 0;
	private string _deleteServiceScheduleName = string.Empty;

	private int _recoverServiceScheduleId = 0;
	private string _recoverServiceScheduleName = string.Empty;

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
			.Add(ModCode.Ctrl, Code.S, SaveServiceSchedule, "Save", Exclude.None)
			.Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
			.Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
			.Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
			.Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
			.Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
			.Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
			.Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
			.Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

		_serviceSchedules = await CommonData.LoadTableData<ServiceScheduleModel>(TableNames.ServiceSchedule);
		_serviceTypes = await CommonData.LoadTableData<ServiceTypeModel>(TableNames.ServiceType);
		_vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(TableNames.VehicleType);

		// Filter only active service types and vehicle types
		_serviceTypes = [.. _serviceTypes.Where(st => st.Status)];
		_vehicleTypes = [.. _vehicleTypes.Where(vt => vt.Status)];

		if (!_showDeleted)
			_serviceSchedules = [.. _serviceSchedules.Where(l => l.Status)];

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion

	#region Actions
	private void OnEditServiceSchedule(ServiceScheduleModel serviceSchedule)
	{
		_serviceSchedule = new()
		{
			Id = serviceSchedule.Id,
			ServiceTypeId = serviceSchedule.ServiceTypeId,
			VehicleTypeId = serviceSchedule.VehicleTypeId,
			IntervalDays = serviceSchedule.IntervalDays,
			Status = serviceSchedule.Status
		};

		StateHasChanged();
	}

	private async Task ShowDeleteConfirmation(int id, string serviceTypeName, string vehicleTypeName)
	{
		_deleteServiceScheduleId = id;
		_deleteServiceScheduleName = $"{serviceTypeName} - {vehicleTypeName}";
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteServiceScheduleId = 0;
		_deleteServiceScheduleName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}

	private async Task ConfirmDelete()
	{
		try
		{
			_isProcessing = true;
			await _deleteConfirmationDialog.HideAsync();

			var serviceSchedule = _serviceSchedules.FirstOrDefault(l => l.Id == _deleteServiceScheduleId);
			if (serviceSchedule == null)
			{
				await _toastNotification.ShowAsync("Error", "Service Schedule not found.", ToastType.Error);
				return;
			}

			serviceSchedule.Status = false;
			await ServiceScheduleData.InsertServiceSchedule(serviceSchedule);

			await _toastNotification.ShowAsync("Deleted", $"Service Schedule '{_deleteServiceScheduleName}' has been deleted successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.AdminServiceSchedule, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete Service Schedule: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteServiceScheduleId = 0;
			_deleteServiceScheduleName = string.Empty;
		}
	}

	private async Task ShowRecoverConfirmation(int id, string serviceTypeName, string vehicleTypeName)
	{
		_recoverServiceScheduleId = id;
		_recoverServiceScheduleName = $"{serviceTypeName} - {vehicleTypeName}";
		await _recoverConfirmationDialog.ShowAsync();
	}

	private async Task CancelRecover()
	{
		_recoverServiceScheduleId = 0;
		_recoverServiceScheduleName = string.Empty;
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

			var serviceSchedule = _serviceSchedules.FirstOrDefault(l => l.Id == _recoverServiceScheduleId);
			if (serviceSchedule == null)
			{
				await _toastNotification.ShowAsync("Error", "Service Schedule not found.", ToastType.Error);
				return;
			}

			serviceSchedule.Status = true;
			await ServiceScheduleData.InsertServiceSchedule(serviceSchedule);

			await _toastNotification.ShowAsync("Recovered", $"Service Schedule '{_recoverServiceScheduleName}' has been recovered successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.AdminServiceSchedule, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to recover Service Schedule: {ex.Message}", ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_recoverServiceScheduleId = 0;
			_recoverServiceScheduleName = string.Empty;
		}
	}
	#endregion

	#region Saving
	private async Task<bool> ValidateForm()
	{
		_serviceSchedule.Status = true;

		if (_serviceSchedule.ServiceTypeId <= 0)
		{
			await _toastNotification.ShowAsync("Validation", "Service Type is required. Please select a valid service type.", ToastType.Warning);
			return false;
		}

		if (_serviceSchedule.VehicleTypeId <= 0)
		{
			await _toastNotification.ShowAsync("Validation", "Vehicle Type is required. Please select a valid vehicle type.", ToastType.Warning);
			return false;
		}

		if (_serviceSchedule.IntervalDays <= 0)
		{
			await _toastNotification.ShowAsync("Validation", "Interval Days must be greater than 0. Please enter a valid interval.", ToastType.Warning);
			return false;
		}

		// Check for duplicate combination
		if (_serviceSchedule.Id > 0)
		{
			var existingSchedule = _serviceSchedules.FirstOrDefault(_ => _.Id != _serviceSchedule.Id &&
				_.ServiceTypeId == _serviceSchedule.ServiceTypeId &&
				_.VehicleTypeId == _serviceSchedule.VehicleTypeId);
			if (existingSchedule is not null)
			{
				var serviceTypeName = GetServiceTypeName(_serviceSchedule.ServiceTypeId);
				var vehicleTypeName = GetVehicleTypeName(_serviceSchedule.VehicleTypeId);
				await _toastNotification.ShowAsync("Duplicate", $"Service Schedule for '{serviceTypeName}' and '{vehicleTypeName}' already exists. Please choose a different combination.", ToastType.Warning);
				return false;
			}
		}
		else
		{
			var existingSchedule = _serviceSchedules.FirstOrDefault(_ =>
				_.ServiceTypeId == _serviceSchedule.ServiceTypeId &&
				_.VehicleTypeId == _serviceSchedule.VehicleTypeId);
			if (existingSchedule is not null)
			{
				var serviceTypeName = GetServiceTypeName(_serviceSchedule.ServiceTypeId);
				var vehicleTypeName = GetVehicleTypeName(_serviceSchedule.VehicleTypeId);
				await _toastNotification.ShowAsync("Duplicate", $"Service Schedule for '{serviceTypeName}' and '{vehicleTypeName}' already exists. Please choose a different combination.", ToastType.Warning);
				return false;
			}
		}

		return true;
	}

	private async Task SaveServiceSchedule()
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

			await _toastNotification.ShowAsync("Processing", "Please wait while the service schedule is being saved...", ToastType.Info);

			await ServiceScheduleData.InsertServiceSchedule(_serviceSchedule);

			var serviceTypeName = GetServiceTypeName(_serviceSchedule.ServiceTypeId);
			var vehicleTypeName = GetVehicleTypeName(_serviceSchedule.VehicleTypeId);
			await _toastNotification.ShowAsync("Saved", $"Service Schedule for '{serviceTypeName}' and '{vehicleTypeName}' has been saved successfully.", ToastType.Success);
			NavigationManager.NavigateTo(PageRouteNames.AdminServiceSchedule, true);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to save Service Schedule: {ex.Message}", ToastType.Error);
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

			var (stream, fileName) = await ServiceScheduleExport.ExportMaster(_serviceSchedules, ReportExportType.Excel);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Success", "Service Schedule data exported to Excel successfully.", ToastType.Success);
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

			var (stream, fileName) = await ServiceScheduleExport.ExportMaster(_serviceSchedules, ReportExportType.PDF);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Success", "Service Schedule data exported to PDF successfully.", ToastType.Success);
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
	private string GetServiceTypeName(int serviceTypeId)
	{
		var serviceType = _serviceTypes.FirstOrDefault(st => st.Id == serviceTypeId);
		return serviceType?.Name ?? "Unknown";
	}

	private string GetVehicleTypeName(int vehicleTypeId)
	{
		var vehicleType = _vehicleTypes.FirstOrDefault(vt => vt.Id == vehicleTypeId);
		return vehicleType?.Name ?? "Unknown";
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			OnEditServiceSchedule(selectedRecords[0]);
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
		{
			var serviceSchedule = selectedRecords[0];
			var serviceTypeName = GetServiceTypeName(serviceSchedule.ServiceTypeId);
			var vehicleTypeName = GetVehicleTypeName(serviceSchedule.VehicleTypeId);

			if (serviceSchedule.Status)
				await ShowDeleteConfirmation(serviceSchedule.Id, serviceTypeName, vehicleTypeName);
			else
				await ShowRecoverConfirmation(serviceSchedule.Id, serviceTypeName, vehicleTypeName);
		}
	}

	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.AdminServiceSchedule, true);

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
