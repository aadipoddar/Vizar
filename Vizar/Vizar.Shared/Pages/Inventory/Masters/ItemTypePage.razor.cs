using Syncfusion.Blazor.Grids;

using Vizar.Shared.Components.Dialog;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Masters;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.Masters;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Operations;

namespace Vizar.Shared.Pages.Inventory.Masters;

public partial class ItemTypePage : IAsyncDisposable
{
    private HotKeysContext _hotKeysContext;
    private bool _isLoading = true;
    private bool _isProcessing = false;
    private bool _showDeleted = false;

    private ItemTypeModel _itemType = new();

    private List<ItemTypeModel> _itemTypes = [];

    private SfGrid<ItemTypeModel> _sfGrid;
    private DeleteConfirmationDialog _deleteConfirmationDialog;
    private RecoverConfirmationDialog _recoverConfirmationDialog;

    private int _deleteItemTypeId = 0;
    private string _deleteItemTypeName = string.Empty;

    private int _recoverItemTypeId = 0;
    private string _recoverItemTypeName = string.Empty;

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
            .Add(ModCode.Ctrl, Code.S, SaveItemType, "Save", Exclude.None)
            .Add(ModCode.Ctrl, Code.E, ExportExcel, "Export Excel", Exclude.None)
            .Add(ModCode.Ctrl, Code.P, ExportPdf, "Export PDF", Exclude.None)
            .Add(ModCode.Ctrl, Code.N, ResetPage, "Reset the page", Exclude.None)
            .Add(ModCode.Ctrl, Code.L, Logout, "Logout", Exclude.None)
            .Add(ModCode.Ctrl, Code.B, NavigateBack, "Back", Exclude.None)
            .Add(ModCode.Ctrl, Code.D, NavigateToDashboard, "Dashboard", Exclude.None)
            .Add(Code.Insert, EditSelectedItem, "Edit selected", Exclude.None)
            .Add(Code.Delete, DeleteSelectedItem, "Delete selected", Exclude.None);

        _itemTypes = await CommonData.LoadTableData<ItemTypeModel>(TableNames.ItemType);

        if (!_showDeleted)
            _itemTypes = [.. _itemTypes.Where(l => l.Status)];

        if (_sfGrid is not null)
            await _sfGrid.Refresh();
    }
    #endregion

    #region Actions
    private void OnEditItemType(ItemTypeModel itemType)
    {
        _itemType = new()
        {
            Id = itemType.Id,
            Name = itemType.Name,
            Code = itemType.Code,
            Remarks = itemType.Remarks,
            Status = itemType.Status
        };

        StateHasChanged();
    }

    private async Task ShowDeleteConfirmation(int id, string name)
    {
        _deleteItemTypeId = id;
        _deleteItemTypeName = name;
        await _deleteConfirmationDialog.ShowAsync();
    }

    private async Task CancelDelete()
    {
        _deleteItemTypeId = 0;
        _deleteItemTypeName = string.Empty;
        await _deleteConfirmationDialog.HideAsync();
    }

    private async Task ConfirmDelete()
    {
        try
        {
            _isProcessing = true;
            await _deleteConfirmationDialog.HideAsync();

            var itemType = _itemTypes.FirstOrDefault(l => l.Id == _deleteItemTypeId);
            if (itemType == null)
            {
                await _toastNotification.ShowAsync("Error", "Item Type not found.", ToastType.Error);
                return;
            }

            itemType.Status = false;
            await ItemData.InsertItemType(itemType);

            await _toastNotification.ShowAsync("Deleted", $"Item Type '{itemType.Name}' has been deleted successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItemType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to delete Item Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _deleteItemTypeId = 0;
            _deleteItemTypeName = string.Empty;
        }
    }

    private async Task ShowRecoverConfirmation(int id, string name)
    {
        _recoverItemTypeId = id;
        _recoverItemTypeName = name;
        await _recoverConfirmationDialog.ShowAsync();
    }

    private async Task CancelRecover()
    {
        _recoverItemTypeId = 0;
        _recoverItemTypeName = string.Empty;
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

            var itemType = _itemTypes.FirstOrDefault(l => l.Id == _recoverItemTypeId);
            if (itemType == null)
            {
                await _toastNotification.ShowAsync("Error", "Item Type not found.", ToastType.Error);
                return;
            }

            itemType.Status = true;
            await ItemData.InsertItemType(itemType);

            await _toastNotification.ShowAsync("Recovered", $"Item Type '{itemType.Name}' has been recovered successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItemType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to recover Item Type: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            _recoverItemTypeId = 0;
            _recoverItemTypeName = string.Empty;
        }
    }
    #endregion

    #region Saving
    private async Task<bool> ValidateForm()
    {
        _itemType.Name = _itemType.Name?.Trim() ?? "";
        _itemType.Name = _itemType.Name?.ToUpper() ?? "";

        _itemType.Remarks = _itemType.Remarks?.Trim() ?? "";
        _itemType.Status = true;


        if (_itemType.Id == 0)
            _itemType.Code = await GenerateCodes.GenerateItemTypeCode();

        if (string.IsNullOrWhiteSpace(_itemType.Name))
        {
            await _toastNotification.ShowAsync("Validation", "Item Type name is required. Please enter a valid name.", ToastType.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_itemType.Remarks))
            _itemType.Remarks = null;

        if (_itemType.Id > 0)
        {
            var existingItemType = _itemTypes.FirstOrDefault(_ => _.Id != _itemType.Id && _.Name.Equals(_itemType.Name, StringComparison.OrdinalIgnoreCase));
            if (existingItemType is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Item Type name '{_itemType.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }
        else
        {
            var existingItemType = _itemTypes.FirstOrDefault(_ => _.Name.Equals(_itemType.Name, StringComparison.OrdinalIgnoreCase));
            if (existingItemType is not null)
            {
                await _toastNotification.ShowAsync("Duplicate", $"Item Type name '{_itemType.Name}' already exists. Please choose a different name.", ToastType.Warning);
                return false;
            }
        }

        return true;
    }

    private async Task SaveItemType()
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

            await _toastNotification.ShowAsync("Processing", "Please wait while the type is being saved...", ToastType.Info);

            await ItemData.InsertItemType(_itemType);

            await _toastNotification.ShowAsync("Saved", $"Item Type '{_itemType.Name}' has been saved successfully.", ToastType.Success);
            NavigationManager.NavigateTo(PageRouteNames.AdminItemType, true);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save Item Type: {ex.Message}", ToastType.Error);
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

            var (stream, fileName) = await ItemTypeExport.ExportMaster(_itemTypes, ReportExportType.Excel);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Item Type data exported to Excel successfully.", ToastType.Success);
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

            var (stream, fileName) = await ItemTypeExport.ExportMaster(_itemTypes, ReportExportType.PDF);
            await SaveAndViewService.SaveAndView(fileName, stream);

            await _toastNotification.ShowAsync("Success", "Item Type data exported to PDF successfully.", ToastType.Success);
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
            OnEditItemType(selectedRecords[0]);
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
        NavigationManager.NavigateTo(PageRouteNames.AdminItemType, true);

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.InventoryDashboard);

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