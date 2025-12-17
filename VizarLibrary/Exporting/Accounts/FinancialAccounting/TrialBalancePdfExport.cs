using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.FinancialAccounting;

namespace VizarLibrary.Exporting.Accounts.FinancialAccounting;

public static class TrialBalancePdfExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<TrialBalanceModel> trialBalanceData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        string groupName = null,
        string accountTypeName = null)
    {
        // Define column order based on view type
        List<string> columnOrder;

        // Detailed view - all columns
        if (showAllColumns)
        {
            columnOrder =
            [
                nameof(TrialBalanceModel.LedgerCode),
                nameof(TrialBalanceModel.LedgerName),
                nameof(TrialBalanceModel.OpeningBalance),
                nameof(TrialBalanceModel.OpeningDebit),
                nameof(TrialBalanceModel.OpeningCredit),
                nameof(TrialBalanceModel.Debit),
                nameof(TrialBalanceModel.Credit),
                nameof(TrialBalanceModel.ClosingBalance),
                nameof(TrialBalanceModel.ClosingDebit),
                nameof(TrialBalanceModel.ClosingCredit)
            ];

            if (string.IsNullOrWhiteSpace(groupName))
                columnOrder.Insert(2, nameof(TrialBalanceModel.GroupName));

            if (string.IsNullOrWhiteSpace(accountTypeName))
                columnOrder.Insert(3, nameof(TrialBalanceModel.AccountTypeName));
        }

        // Summary view - essential columns only
        else
            columnOrder =
            [
                nameof(TrialBalanceModel.LedgerName),
                nameof(TrialBalanceModel.OpeningBalance),
                nameof(TrialBalanceModel.Debit),
                nameof(TrialBalanceModel.Credit),
                nameof(TrialBalanceModel.ClosingBalance)
            ];

        // Define column settings with proper formatting
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            // Text fields
            [nameof(TrialBalanceModel.LedgerCode)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(TrialBalanceModel.LedgerName)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(TrialBalanceModel.GroupName)] = new() { DisplayName = "Group", IncludeInTotal = false },
            [nameof(TrialBalanceModel.AccountTypeName)] = new() { DisplayName = "Account Type", IncludeInTotal = false },

            // Numeric fields - Right aligned with totals
            [nameof(TrialBalanceModel.OpeningDebit)] = new()
            {
                DisplayName = "Opening Debit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.OpeningCredit)] = new()
            {
                DisplayName = "Opening Credit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.OpeningBalance)] = new()
            {
                DisplayName = "Opening Balance",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.Debit)] = new()
            {
                DisplayName = "Period Debit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.Credit)] = new()
            {
                DisplayName = "Period Credit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.ClosingDebit)] = new()
            {
                DisplayName = "Closing Debit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.ClosingCredit)] = new()
            {
                DisplayName = "Closing Credit",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            },

            [nameof(TrialBalanceModel.ClosingBalance)] = new()
            {
                DisplayName = "Closing Balance",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = true
            }
        };

        // Call the generic PDF export utility
        // Use landscape for detailed view, portrait for summary view
        var stream = await PDFReportExportUtil.ExportToPdf(
            trialBalanceData,
            "TRIAL BALANCE REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useBuiltInStyle: false,
            logoPath: null,
            useLandscape: showAllColumns,
            new() { ["Group Name"] = groupName ?? null, ["Account Type"] = accountTypeName ?? null }
        );

        string fileName = $"TRIAL_BALANCE";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
