using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Stock;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.ItemIssue;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Data.Inventory.ItemIssue;

public static class ItemIssueData
{
    private static async Task<int> InsertItemIssue(ItemIssueModel itemIssue, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemIssue, itemIssue, sqlDataAccessTransaction)).FirstOrDefault();

    private static async Task<int> InsertItemIssueDetail(ItemIssueDetailModel itemIssueDetail, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemIssueDetail, itemIssueDetail, sqlDataAccessTransaction)).FirstOrDefault();

    public static List<ItemIssueDetailModel> ConvertCartToDetails(List<ItemIssueItemCartModel> cart, int itemIssueId) =>
        [.. cart.Select(item => new ItemIssueDetailModel
        {
            Id = 0,
            MasterId = itemIssueId,
            ItemId = item.ItemId,
            VehicleId = item.VehicleId,
            CurrentHour = item.CurrentHour,
            CurrentKM = item.CurrentKM,
            IdentificationNo = item.IdentificationNo,
            UnitOfMeasurement = item.UnitOfMeasurement,
            Quantity = item.Quantity,
            Rate = item.Rate,
            Total = item.Total,
            Remarks = item.Remarks,
            Status = true
        })];

    public static async Task DeleteTransaction(ItemIssueModel itemIssue)
    {
        using SqlDataAccessTransaction sqlDataAccessTransaction = new();

        try
        {
            sqlDataAccessTransaction.StartTransaction();

            await FinancialYearData.ValidateFinancialYear(itemIssue.TransactionDateTime, sqlDataAccessTransaction);

            itemIssue.Status = false;
            await InsertItemIssue(itemIssue, sqlDataAccessTransaction);
            await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.ItemIssue), itemIssue.Id, sqlDataAccessTransaction);

            sqlDataAccessTransaction.CommitTransaction();

            await ItemIssueNotify.Notify(itemIssue.Id, NotifyType.Deleted);
        }
        catch
        {
            sqlDataAccessTransaction.RollbackTransaction();
            throw;
        }
    }

    public static async Task RecoverTransaction(ItemIssueModel itemIssue)
    {
        itemIssue.Status = true;
        var itemIssueDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, itemIssue.Id);

        await SaveTransaction(itemIssue, null, itemIssueDetails);

        await ItemIssueNotify.Notify(itemIssue.Id, NotifyType.Recovered);
    }

    public static async Task<int> SaveTransaction(ItemIssueModel itemIssue, List<ItemIssueItemCartModel> cart, List<ItemIssueDetailModel> itemIssueDetails = null, bool showNotification = true, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        bool update = itemIssue.Id > 0;

        if (sqlDataAccessTransaction is null)
        {
            (MemoryStream, string)? previousInvoice = null;
            if (update)
                previousInvoice = await ItemIssueInvoiceExport.ExportInvoice(itemIssue.Id, InvoiceExportType.PDF);

            using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

            try
            {
                newSqlDataAccessTransaction.StartTransaction();
                itemIssue.Id = await SaveTransaction(itemIssue, cart, itemIssueDetails, showNotification, newSqlDataAccessTransaction);
                newSqlDataAccessTransaction.CommitTransaction();
            }
            catch
            {
                newSqlDataAccessTransaction.RollbackTransaction();
                throw;
            }

            if (showNotification)
                await ItemIssueNotify.Notify(itemIssue.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

            return itemIssue.Id;
        }

        if (update)
        {
            var existingItemIssue = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, itemIssue.Id, sqlDataAccessTransaction);
            await FinancialYearData.ValidateFinancialYear(existingItemIssue.TransactionDateTime, sqlDataAccessTransaction);
        }

        await FinancialYearData.ValidateFinancialYear(itemIssue.TransactionDateTime, sqlDataAccessTransaction);

        itemIssue.Id = await InsertItemIssue(itemIssue, sqlDataAccessTransaction);
        itemIssueDetails ??= ConvertCartToDetails(cart, itemIssue.Id);
        await SaveTransactionDetail(itemIssue, itemIssueDetails, update, sqlDataAccessTransaction);
        await SaveItemStock(itemIssue, itemIssueDetails, update, sqlDataAccessTransaction);

        return itemIssue.Id;
    }

    private static async Task SaveTransactionDetail(ItemIssueModel itemIssue, List<ItemIssueDetailModel> itemIssueDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (itemIssueDetails is null || itemIssueDetails.Count != itemIssue.TotalItems || itemIssueDetails.Sum(d => d.Quantity) != itemIssue.TotalQuantity)
            throw new InvalidOperationException("Item issue details do not match the transaction summary.");

        if (itemIssueDetails.Any(d => !d.Status))
            throw new InvalidOperationException("Item issue detail items must be active.");

        if (update)
        {
            var existingItemIssueDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, itemIssue.Id, sqlDataAccessTransaction);
            foreach (var item in existingItemIssueDetails)
            {
                item.Status = false;
                await InsertItemIssueDetail(item, sqlDataAccessTransaction);
            }
        }

        foreach (var item in itemIssueDetails)
        {
            item.MasterId = itemIssue.Id;
            var id = await InsertItemIssueDetail(item, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save item issue detail item.");
        }
    }

    private static async Task SaveItemStock(ItemIssueModel itemIssue, List<ItemIssueDetailModel> itemIssueDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (update)
            await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.ItemIssue), itemIssue.Id, sqlDataAccessTransaction);

        foreach (var item in itemIssueDetails)
        {
            var id = await ItemStockData.InsertItemStock(new()
            {
                Id = 0,
                ItemId = item.ItemId,
                IdentificationNo = item.IdentificationNo,
                Quantity = -item.Quantity,
                NetRate = null,
                Type = nameof(StockType.ItemIssue),
                TransactionId = itemIssue.Id,
                TransactionNo = itemIssue.TransactionNo,
                TransactionDateTime = itemIssue.TransactionDateTime
            }, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save item stock entry.");
        }
    }
}