using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class ItemIssueInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(
		int transactionId,
		InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var transactionDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, transaction.Id);
		if (transactionDetails is null || transactionDetails.Count == 0)
			throw new InvalidOperationException("No transaction details found for the transaction.");

		var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId) ??
			throw new InvalidOperationException("Company information is missing.");

		LedgerModel? garageLedger = null;

		if (transaction.GarageId.HasValue)
		{
			var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId.Value);
			garageLedger = new() { Name = garage?.Name ?? "N/A" };
		}

		var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);
		var allVehicles = await CommonData.LoadTableData<VehicleModel>(TableNames.Vehicle);

		var lineItems = transactionDetails.Select(detail =>
		{
			var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
			var vehicle = allVehicles.FirstOrDefault(v => v.Id == detail.VehicleId);
			return new ItemIssueItemCartModel
			{
				ItemId = detail.ItemId,
				ItemName = item?.Name ?? $"Item #{detail.ItemId}",
				IdentificationNo = detail.IdentificationNo,
				UnitOfMeasurement = detail.UnitOfMeasurement,
				Quantity = detail.Quantity,
				Rate = detail.Rate,
				Total = detail.Total,
				VehicleId = detail.VehicleId,
				VehicleCode = vehicle?.Code,
				VehicleShortCode = vehicle?.ShortCode,
				CurrentHour = detail.CurrentHour,
				CurrentKM = detail.CurrentKM,
				Remarks = detail.Remarks,
			};
		}).ToList();

		var invoiceData = new InvoiceData
		{
			TransactionNo = transaction.TransactionNo,
			TransactionDateTime = transaction.TransactionDateTime,
			TotalAmount = transaction.TotalAmount,
			Remarks = transaction.Remarks,
			Status = transaction.Status,
			PaymentModes = null,
			Company = company,
			BillTo = garageLedger,
			InvoiceType = "ITEM ISSUE INVOICE"
		};

		var summaryFields = new Dictionary<string, string>
		{
			["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, pdfWidth: 25, excelWidth: 5),
			new(nameof(ItemIssueItemCartModel.ItemName), "Item", exportType, CellAlignment.Left, pdfWidth: 0, excelWidth: 30),
			new(nameof(ItemIssueItemCartModel.VehicleShortCode), "Vehicle", exportType, CellAlignment.Center, pdfWidth: 40, excelWidth: 15),
			new(nameof(ItemIssueItemCartModel.IdentificationNo), "Identification", exportType, CellAlignment.Center, pdfWidth: 50, excelWidth: 15),
			new(nameof(ItemIssueItemCartModel.UnitOfMeasurement), "UOM", exportType, CellAlignment.Center, pdfWidth: 30, excelWidth: 15),
			new(nameof(ItemIssueItemCartModel.CurrentHour), "Current Hour", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 15, "#,##0.00"),
			new(nameof(ItemIssueItemCartModel.CurrentKM), "Current KM", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 15, "#,##0.00"),
			new(nameof(ItemIssueItemCartModel.Quantity), "Qty", exportType, CellAlignment.Right, pdfWidth: 40, excelWidth: 10, "#,##0.00"),
			new(nameof(ItemIssueItemCartModel.Rate), "Rate", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 12, "#,##0.00"),
			new(nameof(ItemIssueItemCartModel.Total), "Total", exportType, CellAlignment.Right, pdfWidth: 55, excelWidth: 15, "#,##0.00")
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"ITEM_ISSUE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == InvoiceExportType.PDF)
		{
			var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
				invoiceData,
				lineItems,
				columnSettings,
				null,
				summaryFields
			);

			return (stream, fileName + ".pdf");
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

			return (stream, fileName + ".xlsx");
		}
	}
}
