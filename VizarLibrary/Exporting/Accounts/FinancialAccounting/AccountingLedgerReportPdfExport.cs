using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.FinancialAccounting;

namespace VizarLibrary.Exporting.Accounts.FinancialAccounting;

public static class AccountingLedgerReportPdfExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<AccountingLedgerOverviewModel> ledgerData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        string companyName = null,
        string ledgerName = null,
        TrialBalanceModel trialBalance = null)
    {
        // Define column order based on view type
        List<string> columnOrder;

        // Detailed view - all columns
        if (showAllColumns)
        {
            columnOrder =
            [
                nameof(AccountingLedgerOverviewModel.LedgerName),
                nameof(AccountingLedgerOverviewModel.LedgerCode),
                nameof(AccountingLedgerOverviewModel.AccountTypeName),
                nameof(AccountingLedgerOverviewModel.GroupName),
                nameof(AccountingLedgerOverviewModel.TransactionNo),
                nameof(AccountingLedgerOverviewModel.TransactionDateTime),
                nameof(AccountingLedgerOverviewModel.CompanyName),
                nameof(AccountingLedgerOverviewModel.ReferenceType),
                nameof(AccountingLedgerOverviewModel.ReferenceNo),
                nameof(AccountingLedgerOverviewModel.ReferenceDateTime),
                nameof(AccountingLedgerOverviewModel.ReferenceAmount),
                nameof(AccountingLedgerOverviewModel.Debit),
                nameof(AccountingLedgerOverviewModel.Credit),
                nameof(AccountingLedgerOverviewModel.AccountingRemarks),
                nameof(AccountingLedgerOverviewModel.Remarks)
            ];

            if (string.IsNullOrWhiteSpace(companyName))
                columnOrder.Insert(6, nameof(AccountingLedgerOverviewModel.CompanyName));
        }

        // Summary view - essential columns only
        else
            columnOrder =
            [
                nameof(AccountingLedgerOverviewModel.LedgerName),
                nameof(AccountingLedgerOverviewModel.TransactionNo),
                nameof(AccountingLedgerOverviewModel.TransactionDateTime),
                nameof(AccountingLedgerOverviewModel.ReferenceNo),
                nameof(AccountingLedgerOverviewModel.Debit),
                nameof(AccountingLedgerOverviewModel.Credit)
            ];

        // Define column settings with proper formatting
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            // Text fields
            [nameof(AccountingLedgerOverviewModel.LedgerName)] = new() { DisplayName = "Ledger", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.LedgerCode)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.AccountTypeName)] = new() { DisplayName = "Account Type", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.GroupName)] = new() { DisplayName = "Group", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.ReferenceType)] = new() { DisplayName = "Ref Type", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.ReferenceNo)] = new() { DisplayName = "Ref No", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.AccountingRemarks)] = new() { DisplayName = "Accounting Remarks", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.Remarks)] = new() { DisplayName = "Ledger Remarks", IncludeInTotal = false },

            // Date fields with formatting
            [nameof(AccountingLedgerOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", IncludeInTotal = false },
            [nameof(AccountingLedgerOverviewModel.ReferenceDateTime)] = new() { DisplayName = "Ref Date", Format = "dd-MMM-yyyy hh:mm tt", IncludeInTotal = false },

            // Numeric fields - Right aligned with totals
            [nameof(AccountingLedgerOverviewModel.Debit)] = new()
            {
                DisplayName = "Debit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(AccountingLedgerOverviewModel.Credit)] = new()
            {
                DisplayName = "Credit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(AccountingLedgerOverviewModel.ReferenceAmount)] = new()
            {
                DisplayName = "Ref Amount",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            }
        };

        // Prepare custom summary fields if trial balance is provided
        Dictionary<string, string> customSummaryFields = null;
        if (trialBalance != null)
            customSummaryFields = new Dictionary<string, string>
            {
                ["Opening Balance"] = $"₹ {trialBalance.OpeningBalance:N2}",
                ["Closing Balance"] = $"₹ {trialBalance.ClosingBalance:N2}"
            };

        // Call the generic PDF export utility
        // Use landscape only when showing all columns, portrait for summary view
        var stream = await PDFReportExportUtil.ExportToPdf(
            ledgerData,
            "FINANCIAL LEDGER REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useBuiltInStyle: false,
            autoAdjustColumnWidth: true,
            logoPath: null,
            useLandscape: showAllColumns,
            new() { ["Company"] = companyName ?? null, ["Ledger"] = ledgerName ?? null },
            customSummaryFields: customSummaryFields
        );

        string fileName = $"LEDGER_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
