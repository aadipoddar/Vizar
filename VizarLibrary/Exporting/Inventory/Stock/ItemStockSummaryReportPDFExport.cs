using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Stock;

/// <summary>
/// PDF export functionality for Item Stock Report
/// </summary>
public static class ItemStockSummaryReportPDFExport
{
    /// <summary>
    /// Export Item Stock Report to PDF with custom column order and formatting
    /// </summary>
    /// <param name="stockData">Collection of item stock summary records</param>
    /// <param name="dateRangeStart">Start date of the report</param>
    /// <param name="dateRangeEnd">End date of the report</param>
    /// <param name="showAllColumns">Whether to include all columns or just summary columns</param>
    /// <returns>MemoryStream containing the PDF file and the file name</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ItemStockSummaryModel> stockData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true)
    {
        // Define custom column settings matching Excel export
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>();

        // Define column order based on visibility setting (matching Excel export)
        List<string> columnOrder;

        // All columns - detailed view (matching Excel export)
        if (showAllColumns)
            columnOrder =
            [
                nameof(ItemStockSummaryModel.ItemName),
                nameof(ItemStockSummaryModel.ItemCode),
                nameof(ItemStockSummaryModel.ItemTypeName),
                nameof(ItemStockSummaryModel.ItemCategoryName),
                nameof(ItemStockSummaryModel.ManufacturerName),
                nameof(ItemStockSummaryModel.UnitOfMeasurement),
                nameof(ItemStockSummaryModel.OpeningStock),
                nameof(ItemStockSummaryModel.PurchaseStock),
                nameof(ItemStockSummaryModel.SaleStock),
                nameof(ItemStockSummaryModel.MonthlyStock),
                nameof(ItemStockSummaryModel.ClosingStock),
                nameof(ItemStockSummaryModel.Rate),
                nameof(ItemStockSummaryModel.ClosingValue),
                nameof(ItemStockSummaryModel.AveragePrice),
                nameof(ItemStockSummaryModel.WeightedAverageValue),
                nameof(ItemStockSummaryModel.LastPurchasePrice),
                nameof(ItemStockSummaryModel.LastPurchaseValue)
            ];
        // Summary columns - key fields only (matching Excel export)
        else
            columnOrder =
            [
                nameof(ItemStockSummaryModel.ItemName),
                nameof(ItemStockSummaryModel.UnitOfMeasurement),
                nameof(ItemStockSummaryModel.OpeningStock),
                nameof(ItemStockSummaryModel.PurchaseStock),
                nameof(ItemStockSummaryModel.SaleStock),
                nameof(ItemStockSummaryModel.ClosingStock),
                nameof(ItemStockSummaryModel.Rate),
                nameof(ItemStockSummaryModel.ClosingValue)
            ];

        // Customize specific columns for PDF display (matching Excel column names)
        columnSettings[nameof(ItemStockSummaryModel.ItemName)] = new() { DisplayName = "Item Name", IncludeInTotal = false };
        columnSettings[nameof(ItemStockSummaryModel.ItemCode)] = new() { DisplayName = "Code", IncludeInTotal = false };
        columnSettings[nameof(ItemStockSummaryModel.ItemTypeName)] = new() { DisplayName = "Type", IncludeInTotal = false };
        columnSettings[nameof(ItemStockSummaryModel.ItemCategoryName)] = new() { DisplayName = "Category", IncludeInTotal = false };
        columnSettings[nameof(ItemStockSummaryModel.ManufacturerName)] = new() { DisplayName = "Manufacturer", IncludeInTotal = false };
        columnSettings[nameof(ItemStockSummaryModel.UnitOfMeasurement)] = new()
        {
            DisplayName = "UOM",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        // Stock quantity fields - All with totals
        columnSettings[nameof(ItemStockSummaryModel.OpeningStock)] = new()
        {
            DisplayName = "Opening Stock",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.PurchaseStock)] = new()
        {
            DisplayName = "Purchase Stock",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.SaleStock)] = new()
        {
            DisplayName = "Sale Stock",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.MonthlyStock)] = new()
        {
            DisplayName = "Monthly Stock",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.ClosingStock)] = new()
        {
            DisplayName = "Closing Stock",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        // Rate/Price fields - Right aligned, no totals
        columnSettings[nameof(ItemStockSummaryModel.Rate)] = new()
        {
            DisplayName = "Rate",
            Format = "#,##0.00",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.AveragePrice)] = new()
        {
            DisplayName = "Average Price",
            Format = "#,##0.00",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.LastPurchasePrice)] = new()
        {
            DisplayName = "Last Purchase Price",
            Format = "#,##0.00",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        // Value fields - All with totals
        columnSettings[nameof(ItemStockSummaryModel.ClosingValue)] = new()
        {
            DisplayName = "Closing Value",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.WeightedAverageValue)] = new()
        {
            DisplayName = "Weighted Avg Value",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockSummaryModel.LastPurchaseValue)] = new()
        {
            DisplayName = "Last Purchase Value",
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
            stockData,
            "ITEM STOCK REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns  // Use landscape when showing all columns
        );

        string summaryFileName = $"ITEM_STOCK_SUMMARY";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            summaryFileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        summaryFileName += ".pdf";

        return (stream, summaryFileName);
    }
}
