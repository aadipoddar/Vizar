using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

/// <summary>
/// Convert Purchase data to Invoice PDF format
/// </summary>
public static class PurchaseInvoicePDFExport
{
    /// <summary>
    /// Export Purchase as a professional invoice PDF (automatically loads item names)
    /// </summary>
    /// <param name="purchaseHeader">Purchase header data</param>
    /// <param name="purchaseDetails">Purchase detail line items</param>
    /// <param name="company">Company information</param>
    /// <param name="party">Party/Supplier information</param>
    /// <param name="logoPath">Optional: Path to company logo</param>
    /// <param name="invoiceType">Type of document (PURCHASE INVOICE, PURCHASE ORDER, etc.)</param>
    /// <returns>MemoryStream containing the PDF file</returns>
    public static async Task<MemoryStream> ExportPurchaseInvoice(
        PurchaseModel purchaseHeader,
        List<PurchaseDetailModel> purchaseDetails,
        CompanyModel company,
        LedgerModel party,
        string logoPath = null,
        string invoiceType = "PURCHASE INVOICE")
    {
        // Load all items to get names and create enriched line items
        var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

        var lineItems = purchaseDetails.Select(detail =>
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
        var invoiceData = new PDFInvoiceExportUtil.InvoiceData
        {
            TransactionNo = purchaseHeader.TransactionNo,
            TransactionDateTime = purchaseHeader.TransactionDateTime,
            TotalAmount = purchaseHeader.TotalAmount,
            Remarks = purchaseHeader.Remarks,
            Status = purchaseHeader.Status,
            PaymentModes = null // Purchase invoices typically don't show payment breakdown
        };

        // Define custom summary fields for purchase invoice
        var summaryFields = new Dictionary<string, string>
        {
            ["Items Total"] = purchaseHeader.TotalAfterTax.FormatIndianCurrency(),
            ["Other Charges"] = $"({purchaseHeader.OtherChargesPercent:0.00}%) {purchaseHeader.OtherChargesAmount.FormatIndianCurrency()}",
            ["Cash Discount"] = $"({purchaseHeader.CashDiscountPercent:0.00}%) -{purchaseHeader.CashDiscountAmount.FormatIndianCurrency()}",
            ["Round Off"] = purchaseHeader.RoundOffAmount.FormatIndianCurrency(),
            ["Grand Total"] = purchaseHeader.TotalAmount.FormatIndianCurrency()
		};

        // Define custom column settings with proper display names
        var columnSettings = new List<PDFInvoiceExportUtil.InvoiceColumnSetting>
        {
			new("#", "#", 25, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
			new(nameof(PurchaseItemCartModel.ItemName), "Item", 0, Syncfusion.Pdf.Graphics.PdfTextAlignment.Left),
			new(nameof(PurchaseItemCartModel.IdentificationNo), "Identification", 70, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
			new(nameof(PurchaseItemCartModel.UnitOfMeasurement), "UOM", 40, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
			new(nameof(PurchaseItemCartModel.Quantity), "Qty", 40, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
			new(nameof(PurchaseItemCartModel.Rate), "Rate", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
			new(nameof(PurchaseItemCartModel.DiscountPercent), "Disc %", 45, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
			new(nameof(PurchaseItemCartModel.AfterDiscount), "Taxable", 55, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
			new(nameof(PurchaseItemCartModel.TotalTaxAmount), "Tax", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
			new(nameof(PurchaseItemCartModel.Total), "Total", 55, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00")
		};

        // Generate invoice PDF with custom columns and summary
        return await PDFInvoiceExportUtil.ExportInvoiceToPdf(
            invoiceData,
            lineItems,
            company,
            party,
            logoPath,
            invoiceType,
            columnSettings,
            null, // Column order derived from settings
            summaryFields
        );
    }
}
