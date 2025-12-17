using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Data.Inventory.Item;

public static class ItemStockData
{
    public static async Task<int> InsertItemStock(ItemStockModel stock) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemStock, stock)).FirstOrDefault();

    public static async Task<List<ItemStockSummaryModel>> LoadItemStockSummaryByDate(DateTime StartDate, DateTime EndDate) =>
        await SqlDataAccess.LoadData<ItemStockSummaryModel, dynamic>(StoredProcedureNames.LoadItemStockSummaryByDate, new { StartDate, EndDate });

    public static async Task DeleteItemStockByTypeTransactionId(string Type, int TransactionId) =>
        await SqlDataAccess.SaveData(StoredProcedureNames.DeleteItemStockByTypeTransactionId, new { Type, TransactionId });

    public static async Task DeleteItemStockById(int Id)
    {
        var stock = await CommonData.LoadTableDataById<ItemStockModel>(TableNames.ItemStock, Id);
        if (stock is null)
            return;

        var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(stock.TransactionDateTime);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new Exception("Cannot delete stock entry as the financial year is locked.");

        await SqlDataAccess.SaveData(StoredProcedureNames.DeleteItemStockById, new { Id });
    }

    public static async Task SaveItemStockAdjustment(DateTime transactionDateTime, List<ItemStockAdjustmentCartModel> cart)
    {
        var transactionNo = await GenerateCodes.GenerateItemStockAdjustmentTransactionNo(transactionDateTime);
        var stockSummary = await LoadItemStockSummaryByDate(transactionDateTime, transactionDateTime);

        var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(transactionDateTime);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new Exception("Cannot delete stock entry as the financial year is locked.");

        foreach (var item in cart)
        {
            decimal adjustmentQuantity = 0;
            var existingStock = stockSummary.FirstOrDefault(s => s.ItemId == item.ItemId);

            if (existingStock is null)
                adjustmentQuantity = item.Quantity;
            else
                adjustmentQuantity = item.Quantity - existingStock.ClosingStock;

            if (adjustmentQuantity != 0)
                await InsertItemStock(new()
                {
                    Id = 0,
                    ItemId = item.ItemId,
                    IdentificationNo = item.IdentificationNo,
                    Quantity = adjustmentQuantity,
                    NetRate = null,
                    TransactionId = null,
                    Type = nameof(StockType.Adjustment),
                    TransactionNo = transactionNo,
                    TransactionDateTime = transactionDateTime
                });
        }
    }
}