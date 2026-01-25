using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class InsideRepairInvoiceExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(
        int transactionId,
        InvoiceExportType exportType)
    {
        var transaction = await CommonData.LoadTableDataById<InsideRepairModel>(TableNames.InsideRepair, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        var transactionDetails = await CommonData.LoadTableDataByMasterId<InsideRepairDetailModel>(TableNames.InsideRepairDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId) ??
            throw new InvalidOperationException("Company information is missing.");

        var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId);
        var vehicle = await CommonData.LoadTableDataById<VehicleModel>(TableNames.Vehicle, transaction.VehicleId);
        var address = $"{(transaction.CurrentHour is not null ? $"Current Hour: {transaction.CurrentHour}\n" : "")}" +
                      $"{(transaction.CurrentKM is not null ? $"Current KM: {transaction.CurrentKM}\n" : "")}";
        LedgerModel vehicleLedger = new() { Name = vehicle.Code, Address = address };

        var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

        var lineItems = transactionDetails.Select(detail =>
        {
            var item = allItems.FirstOrDefault(i => i.Id == detail.ItemId);
            return new InsideRepairItemCartModel
            {
                ItemId = detail.ItemId,
                ItemName = item?.Name ?? $"Item #{detail.ItemId}",
                IdentificationNo = detail.IdentificationNo,
                UnitOfMeasurement = detail.UnitOfMeasurement,
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
            Garage = garage.Name,
            BillTo = vehicleLedger,
            InvoiceType = "INSIDE REPAIR INVOICE"
        };

        var summaryFields = new Dictionary<string, string>
        {
            ["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
        };

        var columnSettings = new List<InvoiceColumnSetting>
        {
            new("#", "#", exportType, CellAlignment.Center, pdfWidth: 25, excelWidth: 5),
            new(nameof(InsideRepairItemCartModel.ItemName), "Item", exportType, CellAlignment.Left, pdfWidth: 0, excelWidth: 30),
            new(nameof(InsideRepairItemCartModel.IdentificationNo), "Identification", exportType, CellAlignment.Center, pdfWidth: 50, excelWidth: 15),
            new(nameof(InsideRepairItemCartModel.UnitOfMeasurement), "UOM", exportType, CellAlignment.Center, pdfWidth: 30, excelWidth: 15),
            new(nameof(InsideRepairItemCartModel.Quantity), "Qty", exportType, CellAlignment.Right, pdfWidth: 40, excelWidth: 10, "#,##0.00"),
            new(nameof(InsideRepairItemCartModel.Rate), "Rate", exportType, CellAlignment.Right, pdfWidth: 50, excelWidth: 12, "#,##0.00"),
            new(nameof(InsideRepairItemCartModel.Total), "Total", exportType, CellAlignment.Right, pdfWidth: 55, excelWidth: 15, "#,##0.00")
        };

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"INSIDE_REPAIR_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}";

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
