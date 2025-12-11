using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

/// <summary>
/// Convert Purchase Return data to Invoice Excel format
/// </summary>
public static class PurchaseReturnInvoiceExcelExport
{
	/// <summary>
	/// Export Purchase Return as a professional invoice Excel (automatically loads item names)
	/// </summary>
	/// <param name="purchaseReturnHeader">Purchase return header data</param>
	/// <param name="purchaseReturnDetails">Purchase return detail line items</param>
	/// <param name="company">Company information</param>
	/// <param name="party">Party/Supplier information</param>
	/// <param name="logoPath">Optional: Path to company logo</param>
	/// <param name="invoiceType">Type of document (PURCHASE RETURN, DEBIT NOTE, etc.)</param>
	/// <returns>MemoryStream containing the Excel file</returns>
	public static async Task<MemoryStream> ExportPurchaseReturnInvoice(
		PurchaseReturnModel purchaseReturnHeader,
		List<PurchaseReturnDetailModel> purchaseReturnDetails,
		CompanyModel company,
		LedgerModel party,
		string logoPath = null,
		string invoiceType = "PURCHASE RETURN")
	{
		// Load all items to get names
		var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

		// Map to cart items with actual item names
		var cartItems = purchaseReturnDetails.Select(detail =>
		{
			var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
			string itemName = item?.Name ?? $"Item #{detail.ItemId}";

			return new PurchaseReturnItemCartModel
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

		// Map invoice header data
		var invoiceData = new ExcelInvoiceExportUtil.InvoiceData
		{
			TransactionNo = purchaseReturnHeader.TransactionNo,
			TransactionDateTime = purchaseReturnHeader.TransactionDateTime,
			TotalAmount = purchaseReturnHeader.TotalAmount,
			Remarks = purchaseReturnHeader.Remarks,
			Status = purchaseReturnHeader.Status,
			PaymentModes = null // No payment modes for purchase returns
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

		// Define column settings with # column first
		var columnSettings = new List<ExcelInvoiceExportUtil.InvoiceColumnSetting>
		{
			new("#", "#", 5, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
			new(nameof(PurchaseReturnItemCartModel.ItemName), "Item", 30, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
			new(nameof(PurchaseReturnItemCartModel.IdentificationNo), "Identification", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
			new(nameof(PurchaseReturnItemCartModel.UnitOfMeasurement), "UOM", 8, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
			new(nameof(PurchaseReturnItemCartModel.Quantity), "Qty", 10, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new(nameof(PurchaseReturnItemCartModel.Rate), "Rate", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new(nameof(PurchaseReturnItemCartModel.DiscountPercent), "Disc %", 8, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new(nameof(PurchaseReturnItemCartModel.AfterDiscount), "Taxable", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new(nameof(PurchaseReturnItemCartModel.TotalTaxAmount), "Tax Amt", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new(nameof(PurchaseReturnItemCartModel.Total), "Total", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00")
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
