using Syncfusion.Pdf.Graphics;

using VizarLibrary.Data;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Exporting.Accounts.FinancialAccounting;

/// <summary>
/// Convert Accounting voucher data to Invoice PDF format
/// </summary>
public static class AccountingInvoicePDFExport
{
    /// <summary>
    /// Export Accounting voucher as a professional accounting voucher PDF (automatically loads ledger names)
    /// </summary>
    /// <param name="transactionId">Transaction Id</param>
    /// <returns>MemoryStream containing the PDF file</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportInvoice(int transactionId)
    {
        // Load saved purchase details (since _purchase now has the Id)
        var transaction = await CommonData.LoadTableDataById<AccountingModel>(TableNames.Accounting, transactionId) ??
            throw new InvalidOperationException("Transaction not found.");

        // Load purchase details from database
        var transactionDetails = await CommonData.LoadTableDataByMasterId<AccountingDetailModel>(TableNames.AccountingDetail, transaction.Id);
        if (transactionDetails is null || transactionDetails.Count == 0)
            throw new InvalidOperationException("No transaction details found for the transaction.");

        // Load company information
        var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId) ?? throw new InvalidOperationException("Company information is missing.");

        // Use voucher name as invoice type
        var voucher = await CommonData.LoadTableDataById<VoucherModel>(TableNames.Voucher, transaction.VoucherId);

        // Load all ledgers to get names
        var allLedgers = await CommonData.LoadTableData<LedgerModel>(TableNames.Ledger);

        // Map to cart items with actual ledger names
        var cartItems = transactionDetails.Select(detail =>
        {
            var ledger = allLedgers.FirstOrDefault(l => l.Id == detail.LedgerId);
            return new AccountingItemCartModel
            {
                LedgerId = detail.LedgerId,
                LedgerName = ledger?.Name ?? $"Ledger #{detail.LedgerId}",
                ReferenceNo = detail.ReferenceNo,
                ReferenceType = detail.ReferenceType,
                Debit = detail.Debit,
                Credit = detail.Credit,
                Remarks = detail.Remarks
            };
        }).ToList();

        // Calculate totals
        decimal totalDebit = cartItems.Sum(i => i.Debit ?? 0);
        decimal totalCredit = cartItems.Sum(i => i.Credit ?? 0);
        decimal difference = totalDebit - totalCredit;

        // Map invoice header data
        var invoiceData = new PDFInvoiceExportUtil.InvoiceData
        {
            TransactionNo = transaction.TransactionNo,
            TransactionDateTime = transaction.TransactionDateTime,
            ReferenceTransactionNo = transaction.ReferenceNo,
            TotalAmount = Math.Max(transaction.TotalDebitAmount, transaction.TotalCreditAmount),
            Remarks = transaction.Remarks,
            Status = transaction.Status,
            PaymentModes = null
        };

        // Define custom column settings for accounting vouchers
        var columnSettings = new List<PDFInvoiceExportUtil.InvoiceColumnSetting>
        {
            new("#", "#", 25, PdfTextAlignment.Center),
            new(nameof(AccountingItemCartModel.LedgerName), "Ledger", 0, PdfTextAlignment.Left),
            new(nameof(AccountingItemCartModel.ReferenceNo), "Ref No", 80, PdfTextAlignment.Left),
            new(nameof(AccountingItemCartModel.Debit), "Dr", 70, PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(AccountingItemCartModel.Credit), "Cr", 70, PdfTextAlignment.Right, "#,##0.00"),
            new(nameof(AccountingItemCartModel.Remarks), "Remarks", 100, PdfTextAlignment.Left)
        };

        // Define custom summary fields for accounting
        var summaryFields = new Dictionary<string, string>
        {
            ["Total Debit"] = totalDebit.FormatIndianCurrency(),
            ["Total Credit"] = totalCredit.FormatIndianCurrency(),
            ["Difference"] = difference.FormatIndianCurrency()
        };

        // Use generic invoice export with custom columns
        var stream = await PDFInvoiceExportUtil.ExportInvoiceToPdf(
            invoiceData,
            cartItems,
            company,
            null, // No billTo for accounting vouchers
            voucher.Name.ToUpper(),
            columnSettings,
            null,
            summaryFields
        );

        // Generate file name
        var currentDateTime = await CommonData.LoadCurrentDateTime();
        string fileName = $"ACCOUNTING_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}