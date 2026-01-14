using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

/// <summary>
/// Convert Purchase data to Invoice Excel format
/// </summary>
public static class PurchaseInvoiceExcelExport
{
    /// <summary>
    /// Export Purchase as a professional invoice Excel (automatically loads item names)
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>MemoryStream containing the Excel file</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId)
    {
        // Load saved purchase details (since _purchase now has the Id)
        var transaction = await CommonData.LoadTableDataById<PurchaseModel>(TableNames.Purchase, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        // Load purchase details from database
        var transactionDetails = await CommonData.LoadTableDataByMasterId<PurchaseDetailModel>(TableNames.PurchaseDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        // Load company and party information
        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId);
        var party = await CommonData.LoadTableDataById<LedgerModel>(TableNames.Ledger, transaction.PartyId);
        if (company is null || party is null)
            throw new InvalidOperationException("Company or party information is missing.");

        // Load all items to get names
        var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

        // Map to cart items with actual item names
        var cartItems = transactionDetails.Select(detail =>
        {
            var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
            return new PurchaseItemCartModel
            {
                ItemId = detail.ItemId,
                ItemName = item?.Name ?? $"Item #{detail.ItemId}",
                IdentificationNo = detail.IdentificationNo,
                Quantity = detail.Quantity,
                UnitOfMeasurement = detail.UnitOfMeasurement,
                Rate = detail.Rate,
                DiscountPercent = detail.DiscountPercent,
                AfterDiscount = detail.AfterDiscount,
                CGSTPercent = detail.InclusiveTax ? 0 : detail.CGSTPercent,
                SGSTPercent = detail.InclusiveTax ? 0 : detail.SGSTPercent,
                IGSTPercent = detail.InclusiveTax ? 0 : detail.IGSTPercent,
                TotalTaxAmount = detail.InclusiveTax ? 0 : detail.TotalTaxAmount,
                Total = detail.Total
            };
        }).ToList();

        // Map invoice header data with payment modes dictionary
        var invoiceData = new ExcelInvoiceExportUtil.InvoiceData
        {
            TransactionNo = transaction.TransactionNo,
            TransactionDateTime = transaction.TransactionDateTime,
            TotalAmount = transaction.TotalAmount,
            Remarks = transaction.Remarks,
            Status = transaction.Status,
            PaymentModes = null
        };

        // Define custom summary fields
        var summaryFields = new Dictionary<string, string>
        {
            ["Items Total"] = transaction.TotalAfterTax.FormatIndianCurrency(),
            ["Other Charges"] = $"({transaction.OtherChargesPercent:0.00}%) {transaction.OtherChargesAmount.FormatIndianCurrency()}",
            ["Cash Discount"] = $"({transaction.CashDiscountPercent:0.00}%) -{transaction.CashDiscountAmount.FormatIndianCurrency()}",
            ["Round Off"] = transaction.RoundOffAmount.FormatIndianCurrency(),
            ["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
        };

        // Define column settings with # column first
        var columnSettings = new List<ExcelInvoiceExportUtil.InvoiceColumnSetting>
        {
            new("#", "#", 5, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
            new(nameof(PurchaseItemCartModel.ItemName), "Item", 30, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
            new(nameof(PurchaseItemCartModel.IdentificationNo), "Identification", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
            new(nameof(PurchaseItemCartModel.UnitOfMeasurement), "UOM", 8, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
            new(nameof(PurchaseItemCartModel.Quantity), "Qty", 10, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(PurchaseItemCartModel.Rate), "Rate", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(PurchaseItemCartModel.DiscountPercent), "Disc %", 8, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(PurchaseItemCartModel.AfterDiscount), "Taxable", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(PurchaseItemCartModel.TotalTaxAmount), "Tax Amt", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(PurchaseItemCartModel.Total), "Total", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00")
        };

        // Generate invoice Excel with generic method
        var stream = await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
            invoiceData,
            cartItems,
            company,
            party,
            "PURCHASE INVOICE",
            columnSettings,
            null,
            summaryFields
        );

        // Generate file name
        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"PURCHASE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
