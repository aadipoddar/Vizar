using VizarLibrary.Data;
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
	/// <param name="purchaseHeader">Purchase header data</param>
	/// <param name="purchaseDetails">Purchase detail line items</param>
	/// <param name="company">Company information</param>
	/// <param name="party">Party/Supplier information</param>
	/// <param name="logoPath">Optional: Path to company logo</param>
	/// <param name="invoiceType">Type of document (PURCHASE INVOICE, PURCHASE ORDER, etc.)</param>
	/// <returns>MemoryStream containing the Excel file</returns>
	public static async Task<MemoryStream> ExportPurchaseInvoice(
		PurchaseModel purchaseHeader,
		List<PurchaseDetailModel> purchaseDetails,
		CompanyModel company,
		LedgerModel party,
		string logoPath = null,
		string invoiceType = "PURCHASE INVOICE")
	{
		// Load all items to get names
		var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

		// Map to cart items with actual item names
		var cartItems = purchaseDetails.Select(detail =>
		{
			var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
			string itemName = item?.Name ?? $"Item #{detail.ItemId}";

			return new PurchaseItemCartModel
			{
				ItemId = detail.ItemId,
				ItemName = itemName,
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
		return await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
			invoiceData,
			cartItems,
			company,
			party,
			logoPath,
			invoiceType,
			columnSettings,
			null,
			summaryFields
		);
	}
}
