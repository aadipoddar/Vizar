using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class VehicleIssueItemReportPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<VehicleIssueItemOverviewModel> purchaseReturnItemData,
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
                nameof(VehicleIssueItemOverviewModel.VehicleCode),
                nameof(VehicleIssueItemOverviewModel.VehicleShortCode),
                nameof(VehicleIssueItemOverviewModel.CurrentHour),
                nameof(VehicleIssueItemOverviewModel.CurrentKM),
                nameof(VehicleIssueItemOverviewModel.Quantity),
                nameof(VehicleIssueItemOverviewModel.Total),
                nameof(VehicleIssueItemOverviewModel.PreviousHour),
                nameof(VehicleIssueItemOverviewModel.PreviousKM),
                nameof(VehicleIssueItemOverviewModel.Average)
            ];

        // All columns - detailed view (matching Excel export)
        else if (showAllColumns)
            columnOrder =
            [
                nameof(VehicleIssueItemOverviewModel.ItemName),
                nameof(VehicleIssueItemOverviewModel.ItemCode),
                nameof(VehicleIssueItemOverviewModel.ItemCategoryName),
                nameof(VehicleIssueItemOverviewModel.TransactionNo),
                nameof(VehicleIssueItemOverviewModel.TransactionDateTime),
                nameof(VehicleIssueItemOverviewModel.CompanyName),
                nameof(VehicleIssueItemOverviewModel.VehicleCode),
                nameof(VehicleIssueItemOverviewModel.VehicleShortCode),
                nameof(VehicleIssueItemOverviewModel.CurrentHour),
                nameof(VehicleIssueItemOverviewModel.CurrentKM),
                nameof(VehicleIssueItemOverviewModel.IdentificationNo),
                nameof(VehicleIssueItemOverviewModel.UnitOfMeasurement),
                nameof(VehicleIssueItemOverviewModel.Quantity),
                nameof(VehicleIssueItemOverviewModel.Rate),
                nameof(VehicleIssueItemOverviewModel.Total),
                nameof(VehicleIssueItemOverviewModel.PreviousHour),
                nameof(VehicleIssueItemOverviewModel.PreviousKM),
                nameof(VehicleIssueItemOverviewModel.Average),
                nameof(VehicleIssueItemOverviewModel.ItemIssueRemarks),
                nameof(VehicleIssueItemOverviewModel.Remarks)
            ];

        // Summary columns - key fields only (matching Excel export)
        else
            columnOrder =
            [
                nameof(VehicleIssueItemOverviewModel.ItemName),
                nameof(VehicleIssueItemOverviewModel.ItemCode),
                nameof(VehicleIssueItemOverviewModel.TransactionNo),
                nameof(VehicleIssueItemOverviewModel.TransactionDateTime),
                nameof(VehicleIssueItemOverviewModel.VehicleCode),
                nameof(VehicleIssueItemOverviewModel.VehicleShortCode),
                nameof(VehicleIssueItemOverviewModel.CurrentHour),
                nameof(VehicleIssueItemOverviewModel.CurrentKM),
                nameof(VehicleIssueItemOverviewModel.Quantity),
                nameof(VehicleIssueItemOverviewModel.Rate),
                nameof(VehicleIssueItemOverviewModel.Total),
                nameof(VehicleIssueItemOverviewModel.PreviousHour),
                nameof(VehicleIssueItemOverviewModel.PreviousKM),
                nameof(VehicleIssueItemOverviewModel.Average)
            ];

        // Customize specific columns for PDF display (matching Excel column names)
        columnSettings[nameof(VehicleIssueItemOverviewModel.ItemName)] = new() { DisplayName = "Item", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.VehicleShortCode)] = new() { DisplayName = "Vehicle Short", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification No", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.ItemIssueRemarks)] = new() { DisplayName = "Purchase Return Remarks", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", IncludeInTotal = false };
        columnSettings[nameof(VehicleIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Ident No", IncludeInTotal = false };

        columnSettings[nameof(VehicleIssueItemOverviewModel.CurrentHour)] = new()
        {
            DisplayName = "Current Hour",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleIssueItemOverviewModel.CurrentKM)] = new()
        {
            DisplayName = "Current KM",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleIssueItemOverviewModel.PreviousHour)] = new()
        {
            DisplayName = "Previous Hour",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleIssueItemOverviewModel.PreviousKM)] = new()
        {
            DisplayName = "Previous KM",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleIssueItemOverviewModel.Average)] = new()
        {
            DisplayName = "Average",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleIssueItemOverviewModel.Quantity)] = new()
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

        columnSettings[nameof(VehicleIssueItemOverviewModel.Rate)] = new()
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

        columnSettings[nameof(VehicleIssueItemOverviewModel.Total)] = new()
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
            purchaseReturnItemData,
            "VEHICLE ITEM ISSUE REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns || showSummary  // Use landscape when showing all columns
        );

        string fileName = $"VEHICLE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
