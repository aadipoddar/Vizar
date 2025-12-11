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
	/// <param name="purchaseReturnHeader">Purchase return header data</param>
	/// <param name="purchaseReturnDetails">Purchase return detail line items</param>
	/// <param name="company">Company information</param>
	/// <param name="party">Party/Supplier information</param>
	/// <param name="logoPath">Optional: Path to company logo</param>
	/// <param name="invoiceType">Type of document (PURCHASE RETURN, DEBIT NOTE, etc.)</param>
	/// <returns>MemoryStream containing the PDF file</returns>
	public static async Task<MemoryStream> ExportPurchaseReturnInvoice(
		PurchaseReturnModel purchaseReturnHeader,
		List<PurchaseReturnDetailModel> purchaseReturnDetails,
		CompanyModel company,
		LedgerModel party,
		string logoPath = null,
		string invoiceType = "PURCHASE RETURN")
	{
		// Load all items to get names and create enriched line items
		var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

		var lineItems = purchaseReturnDetails.Select(detail =>
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
			TransactionNo = purchaseReturnHeader.TransactionNo,
			TransactionDateTime = purchaseReturnHeader.TransactionDateTime,
			TotalAmount = purchaseReturnHeader.TotalAmount,
			Remarks = purchaseReturnHeader.Remarks,
			Status = purchaseReturnHeader.Status,
			PaymentModes = null // Purchase returns typically don't show payment breakdown
		};

		// Define custom summary fields for purchase return
		var summaryFields = new Dictionary<string, string>
		{
			["Items Total"] = purchaseReturnHeader.TotalAfterTax.FormatIndianCurrency(),
			["Other Charges"] = $"({purchaseReturnHeader.OtherChargesPercent:0.00}%) {purchaseReturnHeader.OtherChargesAmount.FormatIndianCurrency()}",
			["Cash Discount"] = $"({purchaseReturnHeader.CashDiscountPercent:0.00}%) -{purchaseReturnHeader.CashDiscountAmount.FormatIndianCurrency()}",
			["Round Off"] = purchaseReturnHeader.RoundOffAmount.FormatIndianCurrency(),
			["Grand Total"] = purchaseReturnHeader.TotalAmount.FormatIndianCurrency()
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
