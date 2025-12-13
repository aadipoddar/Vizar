using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class ItemIssueReportPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ItemIssueOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        string garageName = null,
        bool showSummary = false)
    {
        // Define custom column settings matching Excel export
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>();

        // Define column order based on visibility setting (matching Excel export)
        List<string> columnOrder;

        // Summary view - grouped by party with totals
        if (showSummary)
            columnOrder =
            [
                nameof(ItemIssueOverviewModel.GarageName),
                nameof(ItemIssueOverviewModel.TotalItems),
                nameof(ItemIssueOverviewModel.TotalQuantity),
                nameof(ItemIssueOverviewModel.TotalAmount)
            ];

        // All columns - detailed view (matching Excel export)
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(ItemIssueOverviewModel.TransactionNo),
                nameof(ItemIssueOverviewModel.TransactionDateTime),
                nameof(ItemIssueOverviewModel.CompanyName),
                nameof(ItemIssueOverviewModel.FinancialYear),
                nameof(ItemIssueOverviewModel.TotalItems),
                nameof(ItemIssueOverviewModel.TotalQuantity),
                nameof(ItemIssueOverviewModel.TotalAmount),
                nameof(ItemIssueOverviewModel.Remarks),
                nameof(ItemIssueOverviewModel.CreatedByName),
                nameof(ItemIssueOverviewModel.CreatedAt),
                nameof(ItemIssueOverviewModel.CreatedFromPlatform),
                nameof(ItemIssueOverviewModel.LastModifiedByUserName),
                nameof(ItemIssueOverviewModel.LastModifiedAt),
                nameof(ItemIssueOverviewModel.LastModifiedFromPlatform)
            ];

            // Add party column only if not filtering by party
            if (string.IsNullOrEmpty(garageName))
                columnOrder.Insert(3, nameof(ItemIssueOverviewModel.GarageName));
        }

        // Summary columns - key fields only (matching Excel export)
        else
        {
            columnOrder =
            [
                nameof(ItemIssueOverviewModel.TransactionNo),
                nameof(ItemIssueOverviewModel.TransactionDateTime),
                nameof(ItemIssueOverviewModel.TotalQuantity),
                nameof(ItemIssueOverviewModel.TotalAmount)
            ];

            // Add party column only if not filtering by party
            if (string.IsNullOrEmpty(garageName))
                columnOrder.Insert(2, nameof(ItemIssueOverviewModel.GarageName));
        }

        // Customize specific columns for PDF display (matching Excel column names)
        columnSettings[nameof(ItemIssueOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.GarageName)] = new() { DisplayName = "Garage", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(ItemIssueOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", IncludeInTotal = false };

        columnSettings[nameof(ItemIssueOverviewModel.TotalItems)] = new()
        {
            DisplayName = "Items",
            Format = "#,##0",
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemIssueOverviewModel.TotalQuantity)] = new()
        {
            DisplayName = "Qty",
            Format = "#,##0.00",
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemIssueOverviewModel.TotalAmount)] = new()
        {
            DisplayName = "Total",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        // Call the generic PDF export utility with landscape mode for all columns
        var stream = await PDFReportExportUtil.ExportToPdf(
            transactionData,
            "ITEM ISSUE REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns || showSummary,  // Use landscape when showing all columns
            headerMetadata: new() { { "Garage", garageName } }
        );

        string fileName = $"ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
