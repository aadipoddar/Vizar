using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Inventory.Purchase;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Data.Inventory.Purchase;

public static class PurchaseOrderData
{
    private static async Task<int> InsertPurchaseOrder(PurchaseOrderModel purchaseOrder, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseOrder, purchaseOrder, sqlDataAccessTransaction)).FirstOrDefault();

    private static async Task<int> InsertPurchaseOrderDetail(PurchaseOrderDetailModel purchaseOrderDetail, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseOrderDetail, purchaseOrderDetail, sqlDataAccessTransaction)).FirstOrDefault();

    public static async Task<List<PurchaseOrderModel>> LoadPurchaseOrderByGarageVendorPending(int GarageId, int VendorId, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        await SqlDataAccess.LoadData<PurchaseOrderModel, dynamic>(StoredProcedureNames.LoadPurchaseOrderByGarageVendorPending, new { GarageId, VendorId }, sqlDataAccessTransaction);

    public static List<PurchaseOrderDetailModel> ConvertCartToDetails(List<PurchaseOrderItemCartModel> cart, int orderId) =>
        [.. cart.Select(item => new PurchaseOrderDetailModel
            {
                Id = 0,
                MasterId = orderId,
                ItemId = item.ItemId,
                UnitOfMeasurement = item.UnitOfMeasurement,
                Quantity = item.Quantity,
                Remarks = item.Remarks,
                Status = true
            })];

    public static async Task UnlinkPurchaseOrderFromPurchase(PurchaseModel purchase, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        if (purchase.PurchaseOrderId is null or <= 0)
            return;

        var purchaseOrder = await CommonData.LoadTableDataById<PurchaseOrderModel>(TableNames.PurchaseOrder, purchase.PurchaseOrderId.Value, sqlDataAccessTransaction);
        if (purchaseOrder is null || purchaseOrder.Id <= 0)
            throw new InvalidOperationException("Purchase Order not found or is inactive.");

        purchaseOrder.PurchaseId = null;
        await InsertPurchaseOrder(purchaseOrder, sqlDataAccessTransaction);
    }

    public static async Task LinkPurchaseOrderToPurchase(PurchaseModel purchase, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        if (purchase.PurchaseOrderId is null or <= 0)
            return;

        var purchaseOrder = await CommonData.LoadTableDataById<PurchaseOrderModel>(TableNames.PurchaseOrder, purchase.PurchaseOrderId.Value, sqlDataAccessTransaction);
        if (purchaseOrder is null || purchaseOrder.Id <= 0 || !purchaseOrder.Status)
            throw new InvalidOperationException("Purchase Order not found or is inactive.");

        if (purchaseOrder.PurchaseId is not null && purchaseOrder.PurchaseId != purchase.Id)
            throw new InvalidOperationException("Purchase Order is already linked to another purchase.");

        purchaseOrder.PurchaseId = purchase.Id;
        await InsertPurchaseOrder(purchaseOrder, sqlDataAccessTransaction);
    }

    public static async Task DeleteTransaction(PurchaseOrderModel purchaseOrder)
    {
        await FinancialYearData.ValidateFinancialYear(purchaseOrder.TransactionDateTime);

        if (purchaseOrder.PurchaseId is not null && purchaseOrder.PurchaseId > 0)
            throw new InvalidOperationException("Cannot delete order as it is already converted to a sale.");

        using SqlDataAccessTransaction sqlDataAccessTransaction = new();

        try
        {
            sqlDataAccessTransaction.StartTransaction();

            purchaseOrder.Status = false;
            await InsertPurchaseOrder(purchaseOrder, sqlDataAccessTransaction);

            sqlDataAccessTransaction.CommitTransaction();

            await PurchaseOrderNotify.Notify(purchaseOrder.Id, NotifyType.Deleted);
        }
        catch
        {
            sqlDataAccessTransaction.RollbackTransaction();
            throw;
        }
    }

    public static async Task RecoverTransaction(PurchaseOrderModel purchaseOrder)
    {
        purchaseOrder.Status = true;
        var transactionDetails = await CommonData.LoadTableDataByMasterId<PurchaseOrderDetailModel>(TableNames.PurchaseOrderDetail, purchaseOrder.Id);

        await SaveTransaction(purchaseOrder, null, transactionDetails, false);
        await PurchaseOrderNotify.Notify(purchaseOrder.Id, NotifyType.Recovered);
    }

    public static async Task<int> SaveTransaction(PurchaseOrderModel purchaseOrder, List<PurchaseOrderItemCartModel> cart, List<PurchaseOrderDetailModel> purchaseOrderDetails = null, bool showNotification = true, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        bool update = purchaseOrder.Id > 0;

        if (sqlDataAccessTransaction is null)
        {
            (MemoryStream, string)? previousInvoice = null;
            if (update)
                previousInvoice = await PurchaseOrderInvoiceExport.ExportInvoice(purchaseOrder.Id, InvoiceExportType.PDF);

            using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

            try
            {
                newSqlDataAccessTransaction.StartTransaction();
                purchaseOrder.Id = await SaveTransaction(purchaseOrder, cart, purchaseOrderDetails, showNotification, newSqlDataAccessTransaction);
                newSqlDataAccessTransaction.CommitTransaction();
            }
            catch
            {
                newSqlDataAccessTransaction.RollbackTransaction();
                throw;
            }

            if (showNotification)
                await PurchaseOrderNotify.Notify(purchaseOrder.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

            return purchaseOrder.Id;
        }

        if (update)
        {
            var existingPurchaseOrder = await CommonData.LoadTableDataById<PurchaseOrderModel>(TableNames.PurchaseOrder, purchaseOrder.Id, sqlDataAccessTransaction);
            await FinancialYearData.ValidateFinancialYear(existingPurchaseOrder.TransactionDateTime, sqlDataAccessTransaction);

            if (existingPurchaseOrder.PurchaseId is not null && existingPurchaseOrder.PurchaseId > 0)
                throw new InvalidOperationException("Cannot update purchase order as it is already converted to a purchase.");

            purchaseOrder.TransactionNo = existingPurchaseOrder.TransactionNo;
        }
        else
            purchaseOrder.TransactionNo = await GenerateCodes.GeneratePurchaseOrderTransactionNo(purchaseOrder, sqlDataAccessTransaction);

        await FinancialYearData.ValidateFinancialYear(purchaseOrder.TransactionDateTime, sqlDataAccessTransaction);

        purchaseOrder.Id = await InsertPurchaseOrder(purchaseOrder, sqlDataAccessTransaction);
        purchaseOrderDetails ??= ConvertCartToDetails(cart, purchaseOrder.Id);
        await SaveTransactionDetail(purchaseOrder, purchaseOrderDetails, update, sqlDataAccessTransaction);

        return purchaseOrder.Id;
    }

    private static async Task SaveTransactionDetail(PurchaseOrderModel purchaseOrder, List<PurchaseOrderDetailModel> purchaseOrderDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (purchaseOrderDetails is null || purchaseOrderDetails.Count != purchaseOrder.TotalItems || purchaseOrderDetails.Sum(od => od.Quantity) != purchaseOrder.TotalQuantity)
            throw new InvalidOperationException("Purchase Order details do not match the purchase order summary.");

        if (purchaseOrderDetails.Any(od => !od.Status))
            throw new InvalidOperationException("Purchase Order detail items must be active.");

        if (update)
        {
            var existingOrderDetails = await CommonData.LoadTableDataByMasterId<PurchaseOrderDetailModel>(TableNames.PurchaseOrderDetail, purchaseOrder.Id, sqlDataAccessTransaction);
            foreach (var item in existingOrderDetails)
            {
                item.Status = false;
                await InsertPurchaseOrderDetail(item, sqlDataAccessTransaction);
            }
        }

        foreach (var item in purchaseOrderDetails)
        {
            item.MasterId = purchaseOrder.Id;
            var id = await InsertPurchaseOrderDetail(item, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save order detail item.");
        }
    }
}
