using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Data.Inventory.ItemIssue;

public static class ItemIssueData
{
    private static async Task<int> InsertItemIssue(ItemIssueModel itemIssue) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemIssue, itemIssue)).FirstOrDefault();

    private static async Task<int> InsertItemIssueDetail(ItemIssueDetailModel itemIssueDetail) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemIssueDetail, itemIssueDetail)).FirstOrDefault();

    public static async Task DeleteTransaction(ItemIssueModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        if (financialYear is null || financialYear.Locked || !financialYear.Status)
            throw new InvalidOperationException("Cannot delete transaction as the financial year is locked.");

        transaction.Status = false;
        await InsertItemIssue(transaction);
        await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.ItemIssue), transaction.Id);
    }

    public static async Task RecoverTransaction(ItemIssueModel transaction)
    {
        var transactionDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, transaction.Id);
        List<ItemIssueItemCartModel> itemIssueItemCarts = [];

        itemIssueItemCarts.AddRange(transactionDetails.Select(item => new ItemIssueItemCartModel()
        {
            ItemId = item.ItemId,
            ItemName = "",
            VehicleId = item.VehicleId,
            VehicleCode = "",
            VehicleShortCode = "",
            CurrentHour = item.CurrentHour,
            CurrentKM = item.CurrentKM,
            IdentificationNo = item.IdentificationNo,
            UnitOfMeasurement = item.UnitOfMeasurement,
            Quantity = item.Quantity,
            Rate = item.Rate,
            Total = item.Total,
            Remarks = item.Remarks
        }));

        await SaveItemIssueTransaction(transaction, itemIssueItemCarts);
    }

    public static async Task<int> SaveItemIssueTransaction(ItemIssueModel itemIssue, List<ItemIssueItemCartModel> itemIssueDetails)
    {
        var update = itemIssue.Id > 0;

        if (update)
        {
            var existingItemIssue = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, itemIssue.Id);
            var updateFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingItemIssue.FinancialYearId);
            if (updateFinancialYear is null || updateFinancialYear.Locked || updateFinancialYear.Status == false)
                throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

            itemIssue.TransactionNo = existingItemIssue.TransactionNo;
        }
        else
            itemIssue.TransactionNo = await GenerateCodes.GenerateItemIssueTransactionNo(itemIssue);

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, itemIssue.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

        itemIssue.Id = await InsertItemIssue(itemIssue);
        await SaveItemIssueDetail(itemIssue, itemIssueDetails, update);
        await SaveItemStock(itemIssue, itemIssueDetails, update);

        return itemIssue.Id;
    }

    private static async Task SaveItemIssueDetail(ItemIssueModel itemIssue,
        List<ItemIssueItemCartModel> itemIssueDetails, bool update)
    {
        if (update)
        {
            var existingItemIssueDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, itemIssue.Id);
            foreach (var item in existingItemIssueDetails)
            {
                item.Status = false;
                await InsertItemIssueDetail(item);
            }
        }

        foreach (var item in itemIssueDetails)
            await InsertItemIssueDetail(new()
            {
                Id = 0,
                MasterId = itemIssue.Id,
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
            });
    }

    private static async Task SaveItemStock(ItemIssueModel itemIssue, List<ItemIssueItemCartModel> cart, bool update)
    {
        if (update)
            await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.ItemIssue), itemIssue.Id);

        foreach (var item in cart)
            await ItemStockData.InsertItemStock(new()
            {
                Id = 0,
                ItemId = item.ItemId,
                Quantity = -item.Quantity,
                IdentificationNo = item.IdentificationNo,
                NetRate = null,
                Type = nameof(StockType.ItemIssue),
                TransactionId = itemIssue.Id,
                TransactionNo = itemIssue.TransactionNo,
                TransactionDateTime = itemIssue.TransactionDateTime
            });
    }
}