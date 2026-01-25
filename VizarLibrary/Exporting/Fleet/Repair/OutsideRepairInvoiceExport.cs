using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class OutsideRepairInvoiceExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(
        int transactionId,
        InvoiceExportType exportType)
    {
        var transaction = await CommonData.LoadTableDataById<OutsideRepairModel>(TableNames.OutsideRepair, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        var transactionDetails = await CommonData.LoadTableDataByMasterId<OutsideRepairDetailModel>(TableNames.OutsideRepairDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId) ??
            throw new InvalidOperationException("Company information is missing.");

        var vendor = await CommonData.LoadTableDataById<LedgerModel>(TableNames.Ledger, transaction.VendorId);
        var vehicle = await CommonData.LoadTableDataById<VehicleModel>(TableNames.Vehicle, transaction.VehicleId);
        vehicle.OpeningHour = transaction.CurrentHour;
        vehicle.OpeningKM = transaction.CurrentKM;

        var lineItems = transactionDetails.Select(detail =>
        {
            return new OutsideRepairItemCartModel
            {
                Job = detail.Job,
                Quantity = detail.Quantity,
                Rate = detail.Rate,
                Total = detail.Total,
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
            BillTo = vendor,
            Vehicle = vehicle,
            ApprovedBy = transaction.ApprovedBy,
            InvoiceType = "OUTSIDE REPAIR INVOICE"
        };

        var summaryFields = new Dictionary<string, string>
        {
            ["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
        };

        var columnSettings = new List<InvoiceColumnSetting>
        {
            new("#", "#", exportType, CellAlignment.Center, pdfWidth: 25, excelWidth: 5),
            new(nameof(OutsideRepairItemCartModel.Job), "Job", exportType, CellAlignment.Left, pdfWidth: 0, excelWidth: 40),
            new(nameof(OutsideRepairItemCartModel.Quantity), "Qty", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 10, "#,##0.00"),
            new(nameof(OutsideRepairItemCartModel.Rate), "Rate", exportType, CellAlignment.Right, pdfWidth: 60, excelWidth: 12, "#,##0.00"),
            new(nameof(OutsideRepairItemCartModel.Total), "Total", exportType, CellAlignment.Right, pdfWidth: 65, excelWidth: 15, "#,##0.00")
        };

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"OUTSIDE_REPAIR_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
