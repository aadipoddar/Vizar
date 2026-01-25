using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class ServiceInvoiceExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(
		int transactionId,
		InvoiceExportType exportType)
	{
		var transaction = await CommonData.LoadTableDataById<ServiceModel>(TableNames.Service, transactionId) ??
			throw new InvalidOperationException("Transaction not found.");

		var transactionDetails = await CommonData.LoadTableDataByMasterId<ServiceItemCartModel>(TableNames.ServiceDetail, transaction.Id);
		if (transactionDetails is null || transactionDetails.Count == 0)
			throw new InvalidOperationException("No transaction details found for the transaction.");

		var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId) ?? throw new InvalidOperationException("Company information is missing.");
		var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId);
		var garageLedger = new LedgerModel() { Name = garage?.Name ?? "N/A" };

		var allItems = await CommonData.LoadTableData<ServiceTypeModel>(TableNames.ServiceType);
		var allVehicles = await CommonData.LoadTableData<VehicleModel>(TableNames.Vehicle);

		var lineItems = transactionDetails.Select(detail =>
		{
			var item = allItems.FirstOrDefault(i => i.Id == detail.ServiceTypeId);
			var vehicle = allVehicles.FirstOrDefault(v => v.Id == detail.VehicleId);
			return new ServiceItemCartModel
			{
				ServiceTypeId = detail.ServiceTypeId,
				ServiceTypeName = item?.Name ?? $"Service #{detail.ServiceTypeId}",
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
			InvoiceType = "SERVICE INVOICE"
		};

		var summaryFields = new Dictionary<string, string>
		{
			["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
		};

		var columnSettings = new List<InvoiceColumnSetting>
		{
			new("#", "#", exportType, CellAlignment.Center, pdfWidth: 25, excelWidth: 5),
			new(nameof(ServiceItemCartModel.ServiceTypeName), "Service", exportType, CellAlignment.Left, pdfWidth: 0, excelWidth: 30),
			new(nameof(ServiceItemCartModel.VehicleCode), "Vehicle", exportType, CellAlignment.Center, pdfWidth: 60, excelWidth: 20),
			new(nameof(ServiceItemCartModel.CurrentHour), "Current Hour", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 15, "#,##0.00"),
			new(nameof(ServiceItemCartModel.CurrentKM), "Current KM", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 15, "#,##0.00"),
			new(nameof(ServiceItemCartModel.Quantity), "Qty", exportType, CellAlignment.Right, pdfWidth: 40, excelWidth: 10, "#,##0.00"),
			new(nameof(ServiceItemCartModel.Rate), "Rate", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 12, "#,##0.00"),
			new(nameof(ServiceItemCartModel.Total), "Total", exportType, CellAlignment.Right, pdfWidth: 55, excelWidth: 15, "#,##0.00")
		};

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		string fileName = $"SERVICE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
