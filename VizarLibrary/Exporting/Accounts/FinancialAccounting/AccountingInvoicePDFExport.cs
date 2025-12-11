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
    /// <param name="accountingHeader">Accounting header data</param>
    /// <param name="accountingDetails">Accounting detail line items (ledger entries)</param>
    /// <param name="company">Company information</param>
    /// <param name="voucher">Voucher type information</param>
    /// <param name="logoPath">Optional: Path to company logo</param>
    /// <param name="invoiceType">Type of document (JOURNAL VOUCHER, PAYMENT VOUCHER, etc.)</param>
    /// <returns>MemoryStream containing the PDF file</returns>
    public static async Task<MemoryStream> ExportAccountingInvoice(
        AccountingModel accountingHeader,
        List<AccountingDetailModel> accountingDetails,
        CompanyModel company,
        VoucherModel voucher,
        string logoPath = null,
        string invoiceType = "ACCOUNTING VOUCHER")
    {
        // Load all ledgers to get names and create enriched cart items
        var allLedgers = await CommonData.LoadTableData<LedgerModel>(TableNames.Ledger);

        var accountingItems = accountingDetails.Select(detail =>
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
        decimal totalDebit = accountingItems.Sum(i => i.Debit ?? 0);
        decimal totalCredit = accountingItems.Sum(i => i.Credit ?? 0);
        decimal difference = totalDebit - totalCredit;

        // Map invoice header data
        var invoiceData = new PDFInvoiceExportUtil.InvoiceData
        {
            TransactionNo = accountingHeader.TransactionNo,
            TransactionDateTime = accountingHeader.TransactionDateTime,
            ReferenceTransactionNo = accountingHeader.ReferenceNo,
            TotalAmount = Math.Max(accountingHeader.TotalDebitAmount, accountingHeader.TotalCreditAmount),
            Remarks = accountingHeader.Remarks,
            Status = accountingHeader.Status,
            PaymentModes = null // Accounting vouchers don't have payment breakdown
        };

        // Use voucher name as invoice type
        string voucherInvoiceType = !string.IsNullOrWhiteSpace(voucher?.Name)
            ? $"{voucher.Name.ToUpper()}"
            : invoiceType;

        // Define custom column settings for accounting vouchers
        var columnSettings = new List<PDFInvoiceExportUtil.InvoiceColumnSetting>
        {
            new("#", "#", 25, PdfTextAlignment.Center),
            new(nameof(AccountingItemCartModel.LedgerName), "Ledger", 0, PdfTextAlignment.Left),
            new(nameof(AccountingItemCartModel.ReferenceNo), "Ref", 80, PdfTextAlignment.Left),
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
        return await PDFInvoiceExportUtil.ExportInvoiceToPdf(
            invoiceData,
            accountingItems,
            company,
            null, // No bill-to for accounting
            logoPath,
            voucherInvoiceType,
            columnSettings,
            null, // Column order derived from settings
            summaryFields
        );
    }
}