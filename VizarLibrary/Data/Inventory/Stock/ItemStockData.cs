using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.Stock;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Data.Inventory.Stock;

public static class ItemStockData
{
    public static async Task<int> InsertItemStock(ItemStockModel stock, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemStock, stock, sqlDataAccessTransaction)).FirstOrDefault();

    public static async Task<List<ItemStockSummaryModel>> LoadItemStockSummaryByGarageDate(int GarageId, DateTime FromDate, DateTime ToDate) =>
        await SqlDataAccess.LoadData<ItemStockSummaryModel, dynamic>(StoredProcedureNames.LoadItemStockSummaryByGarageDate, new { GarageId, FromDate = DateOnly.FromDateTime(FromDate), ToDate = DateOnly.FromDateTime(ToDate) });

    public static async Task DeleteItemStockByTypeTransactionId(string Type, int TransactionId, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        await SqlDataAccess.SaveData(StoredProcedureNames.DeleteItemStockByTypeTransactionId, new { Type, TransactionId }, sqlDataAccessTransaction);

    private static async Task DeleteItemStockById(int Id) =>
        await SqlDataAccess.SaveData(StoredProcedureNames.DeleteItemStockById, new { Id });

    public static async Task DeleteItemStockById(int Id, int userId)
    {
        var stock = await CommonData.LoadTableDataById<ItemStockModel>(TableNames.ItemStock, Id);
        if (stock is null)
            return;

        await FinancialYearData.ValidateFinancialYear(stock.TransactionDateTime);
        await DeleteItemStockById(Id);
        await ItemStockAdjustmentNotify.Notify(stock, userId, NotifyType.Deleted);
    }

    public static async Task SaveItemStockAdjustment(DateTime transactionDateTime, GarageModel garage, List<ItemStockAdjustmentCartModel> cart, int userId)
    {
        var transactionNo = await GenerateCodes.GenerateItemStockAdjustmentTransactionNo(transactionDateTime);
        var stockSummary = await LoadItemStockSummaryByGarageDate(garage.Id, transactionDateTime, transactionDateTime);

        if (cart is null || cart.Count == 0)
            throw new InvalidOperationException("Cannot save stock adjustment with no items.");

        await FinancialYearData.ValidateFinancialYear(transactionDateTime);

        using SqlDataAccessTransaction sqlDataAccessTransaction = new();

        try
        {
            sqlDataAccessTransaction.StartTransaction();

            foreach (var item in cart)
            {
                decimal adjustmentQuantity = 0;
                var existingStock = stockSummary.FirstOrDefault(s => s.ItemId == item.ItemId);

                if (existingStock is null)
                    adjustmentQuantity = item.Quantity;
                else
                    adjustmentQuantity = item.Quantity - existingStock.ClosingStock;

                if (adjustmentQuantity != 0)
                {
                    var id = await InsertItemStock(new()
                    {
                        Id = 0,
                        ItemId = item.ItemId,
                        IdentificationNo = item.IdentificationNo,
                        GarageId = garage.Id,
                        Quantity = adjustmentQuantity,
                        NetRate = null,
                        TransactionId = null,
                        Type = nameof(StockType.Adjustment),
                        TransactionNo = transactionNo,
                        TransactionDateTime = transactionDateTime
                    }, sqlDataAccessTransaction);

                    if (id <= 0)
                        throw new InvalidOperationException($"Failed to insert stock adjustment for raw material ID {item.ItemId}.");
                }
            }

            sqlDataAccessTransaction.CommitTransaction();
        }
        catch
        {
            sqlDataAccessTransaction.RollbackTransaction();
            throw;
        }
    }
}