using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class ServiceInvoicePDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId)
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

        var invoiceData = new PDFInvoiceExportUtil.InvoiceData
        {
            TransactionNo = transaction.TransactionNo,
            TransactionDateTime = transaction.TransactionDateTime,
            TotalAmount = transaction.TotalAmount,
            Remarks = transaction.Remarks,
            Status = transaction.Status,
            PaymentModes = null
        };

        var summaryFields = new Dictionary<string, string>
        {
            ["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
        };

        var columnSettings = new List<PDFInvoiceExportUtil.InvoiceColumnSetting>
        {
            new("#", "#", 25, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
            new(nameof(ServiceItemCartModel.ServiceTypeName), "Service", 0, Syncfusion.Pdf.Graphics.PdfTextAlignment.Left),
            new(nameof(ServiceItemCartModel.VehicleCode), "Vehicle", 60, Syncfusion.Pdf.Graphics.PdfTextAlignment.Center),
            new(nameof(ServiceItemCartModel.CurrentHour), "Current Hour", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(ServiceItemCartModel.CurrentKM), "Current KM", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(ServiceItemCartModel.Quantity), "Qty", 40, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(ServiceItemCartModel.Rate), "Rate", 50, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(ServiceItemCartModel.Total), "Total", 55, Syncfusion.Pdf.Graphics.PdfTextAlignment.Right, "#,##0.00"),
        };

        var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
            invoiceData,
            lineItems,
            company,
            garageLedger,
            "SERVICE INVOICE",
            columnSettings,
            null,
            summaryFields
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"SERVICE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
