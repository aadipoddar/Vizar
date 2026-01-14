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
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>MemoryStream containing the Excel file</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId)
    {
        // Load saved transaction details
        var transaction = await CommonData.LoadTableDataById<PurchaseReturnModel>(TableNames.PurchaseReturn, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        // Load transaction details from database
        var transactionDetails = await CommonData.LoadTableDataByMasterId<PurchaseReturnDetailModel>(TableNames.PurchaseReturnDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        // Load company and party information
        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId);
        var party = await CommonData.LoadTableDataById<LedgerModel>(TableNames.Ledger, transaction.PartyId);
        if (company is null || party is null)
            throw new InvalidOperationException("Company or party information is missing.");

        // Load all items to get names and create enriched line items
        var allItems = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

        var lineItems = transactionDetails.Select(detail =>
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
        var invoiceData = new ExcelInvoiceExportUtil.InvoiceData
        {
            TransactionNo = transaction.TransactionNo,
            TransactionDateTime = transaction.TransactionDateTime,
            TotalAmount = transaction.TotalAmount,
            Remarks = transaction.Remarks,
            Status = transaction.Status,
            PaymentModes = null
        };

        // Define custom summary fields
        var summaryFields = new Dictionary<string, string>
        {
            ["Items Total"] = transaction.TotalAfterTax.FormatIndianCurrency(),
            ["Other Charges"] = $"({transaction.OtherChargesPercent:0.00}%) {transaction.OtherChargesAmount.FormatIndianCurrency()}",
            ["Cash Discount"] = $"({transaction.CashDiscountPercent:0.00}%) -{transaction.CashDiscountAmount.FormatIndianCurrency()}",
            ["Round Off"] = transaction.RoundOffAmount.FormatIndianCurrency(),
            ["Grand Total"] = transaction.TotalAmount.FormatIndianCurrency()
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

        // Generate invoice PDF with custom columns and summary
        var stream = await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
            invoiceData,
            lineItems,
            company,
            party,
            "PURCHASE RETURN INVOICE",
            columnSettings,
            null, // Column order derived from settings
            summaryFields
        );

        // Generate file name
        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"PURCHASE_RETURN_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
