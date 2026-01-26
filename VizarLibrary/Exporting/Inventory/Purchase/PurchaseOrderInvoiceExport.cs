using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

public static class PurchaseOrderInvoiceExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId, InvoiceExportType exportType)
    {
        var transaction = await CommonData.LoadTableDataById<PurchaseOrderModel>(TableNames.PurchaseOrder, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        var transactionDetails = await CommonData.LoadTableDataByMasterId<PurchaseOrderDetailModel>(TableNames.PurchaseOrderDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId);
        var party = await CommonData.LoadTableDataById<LedgerModel>(TableNames.Ledger, transaction.VendorId);
        var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId);
        if (company is null || party is null || garage is null)
            throw new InvalidOperationException("Company or vendor or garage information is missing.");

        PurchaseModel purchase = null;
        if (transaction.PurchaseId is not null && transaction.PurchaseId > 0)
            purchase = await CommonData.LoadTableDataById<PurchaseModel>(TableNames.Purchase, transaction.PurchaseId.Value);

        var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

        var lineItems = transactionDetails.Select(detail =>
        {
            var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
            return new PurchaseOrderItemCartModel
            {
                ItemId = detail.ItemId,
                ItemName = item?.Name ?? $"Item #{detail.ItemId}",
                UnitOfMeasurement = detail.UnitOfMeasurement,
                Quantity = detail.Quantity
            };
        }).ToList();

        var invoiceData = new InvoiceData
        {
            Company = company,
            BillTo = party,
            InvoiceType = "PURCHASE ORDER",
            GarageInfo = garage,
            TransactionNo = transaction.TransactionNo,
            TransactionDateTime = transaction.TransactionDateTime,
            TotalAmount = 0, // Purchase orders don't have amounts
            Remarks = transaction.Remarks ?? string.Empty,
            ReferenceTransactionNo = purchase?.TransactionNo,
            ReferenceDateTime = purchase?.TransactionDateTime,
            Status = transaction.Status,
            PaymentModes = null
        };

        var summaryFields = new Dictionary<string, string>
        {
            ["Total Items"] = transaction.TotalItems.ToString(),
            ["Total Quantity"] = transaction.TotalQuantity.FormatSmartDecimal()
        };

        var columnSettings = new List<InvoiceColumnSetting>
        {
            new("#", "#", exportType, CellAlignment.Center, 25, 5),
            new(nameof(PurchaseOrderItemCartModel.ItemName), "Item", exportType, CellAlignment.Left, 0, 35),
            new(nameof(PurchaseOrderItemCartModel.UnitOfMeasurement), "UOM", exportType, CellAlignment.Center, 50, 12),
            new(nameof(PurchaseOrderItemCartModel.Quantity), "Qty", exportType, CellAlignment.Right, 50, 12, "#,##0.00"),
            new(nameof(PurchaseOrderItemCartModel.Remarks), "Remarks", exportType, CellAlignment.Left, 0, 30)
        };

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"PURCHASE_ORDER_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

        if (exportType == InvoiceExportType.PDF)
        {
            var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
                invoiceData,
                lineItems,
                columnSettings,
                null,
                summaryFields
            );

            fileName += ".pdf";
            return (stream, fileName);
        }
        else
        {
            var stream = await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
                invoiceData,
                lineItems,
                columnSettings,
                null,
                summaryFields
            );

            fileName += ".xlsx";
            return (stream, fileName);
        }
    }
}
