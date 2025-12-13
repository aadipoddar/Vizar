using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

/// <summary>
/// Convert Purchase Return data to Invoice PDF format
/// </summary>
public static class PurchaseReturnInvoicePDFExport
{
    /// <summary>
    /// Export Purchase Return as a professional invoice PDF (automatically loads item names)
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>MemoryStream containing the PDF file</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId)
    {
        // Load saved transaction details
        var transaction = await CommonData.LoadTableDataById<PurchaseReturnModel>(TableNames.PurchaseReturn, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        // Load transaction details from database
        var transactionDetails = await CommonData.LoadTableDataByMasterId<PurchaseReturnDetailModel>(TableNames.PurchaseReturnDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        // Load company and party information
        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId);
        var party = await CommonData.LoadTableDataById<LedgerModel>(TableNames.Ledger, transaction.PartyId);
        if (company is null || party is null)
            throw new InvalidOperationException("Company or party information is missing.");

        // Load all items to get names and create enriched line items
        var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

        var lineItems = transactionDetails.Select(detail =>
        {
            var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
            return new PurchaseReturnItemCartModel
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
        var invoiceData = new PDFInvoiceExportUtil.InvoiceData
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

        // Define custom column settings with proper display names
        var columnSettings = new List<PDFInvoiceExportUtil.InvoiceColumnSetting>
        {
            new("#", "#", 25, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
            new(nameof(PurchaseReturnItemCartModel.ItemName), "Item", 0, Syncfusion.Pdf.Graphics.PdfTextAlignment.Left),
            new(nameof(PurchaseReturnItemCartModel.IdentificationNo), "Identification", 70, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
            new(nameof(PurchaseReturnItemCartModel.UnitOfMeasurement), "UOM", 40, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
            new(nameof(PurchaseReturnItemCartModel.Quantity), "Qty", 40, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(PurchaseReturnItemCartModel.Rate), "Rate", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(PurchaseReturnItemCartModel.DiscountPercent), "Disc %", 45, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(PurchaseReturnItemCartModel.AfterDiscount), "Taxable", 55, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(PurchaseReturnItemCartModel.TotalTaxAmount), "Tax", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(PurchaseReturnItemCartModel.Total), "Total", 55, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00")
        };

        // Generate invoice PDF with custom columns and summary
        var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
            invoiceData,
            lineItems,
            company,
            party,
            "PURCHASE RETURN INVOICE",
            columnSettings,
            null, // Column order derived from settings
            summaryFields
        );

        // Generate file name
        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"PURCHASE_RETURN_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
