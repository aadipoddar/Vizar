using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class GarageIssueItemReportPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<GarageIssueItemOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false)
    {
        // Define custom column settings matching Excel export
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>();

        // Define column order based on showAllColumns and showSummary flags
        List<string> columnOrder;

        // Summary mode - grouped by item with aggregated values
        if (showSummary)
            columnOrder =
            [
                nameof(GarageIssueItemOverviewModel.ItemName),
                nameof(GarageIssueItemOverviewModel.ItemCode),
                nameof(GarageIssueItemOverviewModel.ItemCategoryName),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Total)
            ];

        // All columns - detailed view (matching Excel export)
        else if (showAllColumns)
            columnOrder =
            [
                nameof(GarageIssueItemOverviewModel.ItemName),
                nameof(GarageIssueItemOverviewModel.ItemCode),
                nameof(GarageIssueItemOverviewModel.ItemCategoryName),
                nameof(GarageIssueItemOverviewModel.TransactionNo),
                nameof(GarageIssueItemOverviewModel.TransactionDateTime),
                nameof(GarageIssueItemOverviewModel.CompanyName),
                nameof(GarageIssueItemOverviewModel.GarageName),
                nameof(GarageIssueItemOverviewModel.UnitOfMeasurement),
                nameof(GarageIssueItemOverviewModel.IdentificationNo),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Total),
                nameof(GarageIssueItemOverviewModel.ItemIssueRemarks),
                nameof(GarageIssueItemOverviewModel.Remarks)
            ];

        // Summary columns - key fields only (matching Excel export)
        else
            columnOrder =
            [
                nameof(GarageIssueItemOverviewModel.ItemName),
                nameof(GarageIssueItemOverviewModel.TransactionNo),
                nameof(GarageIssueItemOverviewModel.TransactionDateTime),
                nameof(GarageIssueItemOverviewModel.GarageName),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Rate),
                nameof(GarageIssueItemOverviewModel.Total)
            ];

        // Customize specific columns for PDF display (matching Excel column names)
        columnSettings[nameof(GarageIssueItemOverviewModel.ItemName)] = new() { DisplayName = "Item", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification No", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.ItemIssueRemarks)] = new() { DisplayName = "Purchase Return Remarks", IncludeInTotal = false };
        columnSettings[nameof(GarageIssueItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", IncludeInTotal = false };

        columnSettings[nameof(GarageIssueItemOverviewModel.Quantity)] = new()
        {
            DisplayName = "Qty",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(GarageIssueItemOverviewModel.Rate)] = new()
        {
            DisplayName = "Rate",
            Format = "#,##0.00",
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            },
            IncludeInTotal = false
        };

        columnSettings[nameof(GarageIssueItemOverviewModel.Total)] = new()
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
            "GARAGE ITEM ISSUE REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns || showSummary  // Use landscape when showing all columns
        );

        string fileName = $"GARAGE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
