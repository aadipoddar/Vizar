using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

/// <summary>
/// Convert Item Issue data to Invoice Excel format
/// </summary>
public static class ItemIssueInvoiceExcelExport
{
    /// <summary>
    /// Export Purchase as a professional invoice PDF (automatically loads item names)
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>MemoryStream containing the PDF file</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId)
    {
        // Load saved transaction details
        var transaction = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        // Load transaction details from database
        var transactionDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        // Load company and garage information
        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId) ?? throw new InvalidOperationException("Company information is missing.");

        LedgerModel? garageLedger = null;

        if (transaction.GarageId.HasValue)
        {
            var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId.Value);
            garageLedger = new() { Name = garage?.Name ?? "N/A" };
        }
        else
            garageLedger = null;

        // Load all items to get names and create enriched line items
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

        // Map invoice header data with payment modes dictionary
        var invoiceData = new ExcelInvoiceExportUtil.InvoiceData
        {
            TransactionNo = transaction.TransactionNo,
            TransactionDateTime = transaction.TransactionDateTime,
            TotalAmount = transaction.TotalAmount,
            Remarks = transaction.Remarks,
            Status = transaction.Status,
            PaymentModes = null
        };

        // Define custom summary fields for purchase invoice
        var summaryFields = new Dictionary<string, string>
        {
            ["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
        };

        // Define custom column settings with proper display names
        var columnSettings = new List<ExcelInvoiceExportUtil.InvoiceColumnSetting>
        {
            new("#", "#", 5, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
            new(nameof(ItemIssueItemCartModel.ItemName), "Item", 30, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
            new(nameof(ItemIssueItemCartModel.VehicleShortCode), "Vehicle", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
            new(nameof(ItemIssueItemCartModel.IdentificationNo), "Identification", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
            new(nameof(ItemIssueItemCartModel.UnitOfMeasurement), "UOM", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
            new(nameof(ItemIssueItemCartModel.CurrentHour), "Current Hour", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(ItemIssueItemCartModel.CurrentKM), "Current KM", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(ItemIssueItemCartModel.Quantity), "Qty", 10, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(ItemIssueItemCartModel.Rate), "Rate", 12, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
            new(nameof(ItemIssueItemCartModel.Total), "Total", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
        };

        // Generate invoice PDF with custom columns and summary
        var stream = await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
            invoiceData,
            lineItems,
            company,
            garageLedger,
            "ITEM ISSUE INVOICE",
            columnSettings,
            null, // Column order derived from settings
            summaryFields
        );

        // Generate file name
        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"ITEM_ISSUE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
