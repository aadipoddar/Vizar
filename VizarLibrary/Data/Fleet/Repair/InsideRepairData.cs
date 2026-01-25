using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Stock;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Repair;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Inventory.Stock;

namespace VizarLibrary.Data.Fleet.Repair;

public static class InsideRepairData
{
    private static async Task<int> InsertInsideRepair(InsideRepairModel insideRepair, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertInsideRepair, insideRepair, sqlDataAccessTransaction)).FirstOrDefault();

    private static async Task<int> InsertInsideRepairDetail(InsideRepairDetailModel insideRepairDetail, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertInsideRepairDetail, insideRepairDetail, sqlDataAccessTransaction)).FirstOrDefault();

    public static List<InsideRepairDetailModel> ConvertCartToDetails(List<InsideRepairItemCartModel> cart, int insideRepairId) =>
        [.. cart.Select(item => new InsideRepairDetailModel
        {
            Id = 0,
            MasterId = insideRepairId,
            ItemId = item.ItemId,
            IdentificationNo = item.IdentificationNo,
            UnitOfMeasurement = item.UnitOfMeasurement,
            Quantity = item.Quantity,
            Rate = item.Rate,
            Total = item.Total,
            Remarks = item.Remarks,
            Status = true
        })];

    public static async Task DeleteTransaction(InsideRepairModel insideRepair)
    {
        using SqlDataAccessTransaction sqlDataAccessTransaction = new();

        try
        {
            sqlDataAccessTransaction.StartTransaction();

            await FinancialYearData.ValidateFinancialYear(insideRepair.TransactionDateTime, sqlDataAccessTransaction);

            insideRepair.Status = false;
            await InsertInsideRepair(insideRepair, sqlDataAccessTransaction);
            await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.InsideRepair), insideRepair.Id, sqlDataAccessTransaction);

            sqlDataAccessTransaction.CommitTransaction();

            await InsideRepairNotify.Notify(insideRepair.Id, NotifyType.Deleted);
        }
        catch
        {
            sqlDataAccessTransaction.RollbackTransaction();
            throw;
        }
    }

    public static async Task RecoverTransaction(InsideRepairModel insideRepair)
    {
        insideRepair.Status = true;
        var insideRepairDetails = await CommonData.LoadTableDataByMasterId<InsideRepairDetailModel>(TableNames.InsideRepairDetail, insideRepair.Id);

        await SaveTransaction(insideRepair, null, insideRepairDetails);

        await InsideRepairNotify.Notify(insideRepair.Id, NotifyType.Recovered);
    }

    public static async Task<int> SaveTransaction(InsideRepairModel insideRepair, List<InsideRepairItemCartModel> cart, List<InsideRepairDetailModel> insideRepairDetails = null, bool showNotification = true, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        bool update = insideRepair.Id > 0;

        if (sqlDataAccessTransaction is null)
        {
            (MemoryStream, string)? previousInvoice = null;
            if (update)
                previousInvoice = await InsideRepairInvoiceExport.ExportInvoice(insideRepair.Id, InvoiceExportType.PDF);

            using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

            try
            {
                newSqlDataAccessTransaction.StartTransaction();
                insideRepair.Id = await SaveTransaction(insideRepair, cart, insideRepairDetails, showNotification, newSqlDataAccessTransaction);
                newSqlDataAccessTransaction.CommitTransaction();
            }
            catch
            {
                newSqlDataAccessTransaction.RollbackTransaction();
                throw;
            }

            if (showNotification)
                await InsideRepairNotify.Notify(insideRepair.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

            return insideRepair.Id;
        }

        if (update)
        {
            var existingInsideRepair = await CommonData.LoadTableDataById<InsideRepairModel>(TableNames.InsideRepair, insideRepair.Id, sqlDataAccessTransaction);
            await FinancialYearData.ValidateFinancialYear(existingInsideRepair.TransactionDateTime, sqlDataAccessTransaction);
        }

        await FinancialYearData.ValidateFinancialYear(insideRepair.TransactionDateTime, sqlDataAccessTransaction);

        insideRepair.Id = await InsertInsideRepair(insideRepair, sqlDataAccessTransaction);
        insideRepairDetails ??= ConvertCartToDetails(cart, insideRepair.Id);
        await SaveTransactionDetail(insideRepair, insideRepairDetails, update, sqlDataAccessTransaction);
        await SaveItemStock(insideRepair, insideRepairDetails, update, sqlDataAccessTransaction);

        return insideRepair.Id;
    }

    private static async Task SaveTransactionDetail(InsideRepairModel insideRepair, List<InsideRepairDetailModel> insideRepairDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (insideRepairDetails is null || insideRepairDetails.Count != insideRepair.TotalItems || insideRepairDetails.Sum(d => d.Quantity) != insideRepair.TotalQuantity)
            throw new InvalidOperationException("Item issue details do not match the transaction summary.");

        if (insideRepairDetails.Any(d => !d.Status))
            throw new InvalidOperationException("Item issue detail items must be active.");

        if (update)
        {
            var existingInsideRepairDetails = await CommonData.LoadTableDataByMasterId<InsideRepairDetailModel>(TableNames.InsideRepairDetail, insideRepair.Id, sqlDataAccessTransaction);
            foreach (var item in existingInsideRepairDetails)
            {
                item.Status = false;
                await InsertInsideRepairDetail(item, sqlDataAccessTransaction);
            }
        }

        foreach (var item in insideRepairDetails)
        {
            item.MasterId = insideRepair.Id;
            var id = await InsertInsideRepairDetail(item, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save item issue detail item.");
        }
    }

    private static async Task SaveItemStock(InsideRepairModel insideRepair, List<InsideRepairDetailModel> insideRepairDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (update)
            await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.InsideRepair), insideRepair.Id, sqlDataAccessTransaction);

        foreach (var item in insideRepairDetails)
        {
            var id = await ItemStockData.InsertItemStock(new()
            {
                Id = 0,
                ItemId = item.ItemId,
                IdentificationNo = item.IdentificationNo,
                GarageId = insideRepair.GarageId,
                Quantity = -item.Quantity,
                NetRate = null,
                Type = nameof(StockType.InsideRepair),
                TransactionId = insideRepair.Id,
                TransactionNo = insideRepair.TransactionNo,
                TransactionDateTime = insideRepair.TransactionDateTime
            }, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save item stock entry.");
        }
    }
}
