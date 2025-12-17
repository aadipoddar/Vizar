using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Stock;

/// <summary>
/// Excel export functionality for Item Stock Report
/// </summary>
public static class ItemStockReportExcelExport
{
    /// <summary>
    /// Export Item Stock Report to Excel with custom column order and formatting
    /// </summary>
    /// <param name="stockData">Collection of item stock summary records</param>
    /// <param name="dateRangeStart">Start date of the report</param>
    /// <param name="dateRangeEnd">End date of the report</param>
    /// <param name="showAllColumns">Whether to include all columns or just summary columns</param>
    /// <param name="stockDetailsData">Optional collection of item stock detail records for second worksheet</param>
    /// <returns>MemoryStream containing the Excel file and file name</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ItemStockSummaryModel> stockData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        IEnumerable<ItemStockDetailsModel> stockDetailsData = null)
    {
        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(ItemStockSummaryModel.ItemId)] = new() { DisplayName = "Item ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 12 },
            [nameof(ItemStockSummaryModel.ItemTypeId)] = new() { DisplayName = "Type ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 12 },
            [nameof(ItemStockSummaryModel.ItemCategoryId)] = new() { DisplayName = "Category ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 12 },
            [nameof(ItemStockSummaryModel.ManufacturerId)] = new() { DisplayName = "Manufacturer ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 12 },

            // Text fields
            [nameof(ItemStockSummaryModel.ItemName)] = new() { DisplayName = "Item Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 25 },
            [nameof(ItemStockSummaryModel.ItemCode)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, Width = 15 },
            [nameof(ItemStockSummaryModel.ItemTypeName)] = new() { DisplayName = "Type", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 20 },
            [nameof(ItemStockSummaryModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 20 },
            [nameof(ItemStockSummaryModel.ManufacturerName)] = new() { DisplayName = "Manufacturer", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 20 },
            [nameof(ItemStockSummaryModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, Width = 10 },

            // Stock quantity fields - All with totals
            [nameof(ItemStockSummaryModel.OpeningStock)] = new() { DisplayName = "Opening Stock", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },
            [nameof(ItemStockSummaryModel.PurchaseStock)] = new() { DisplayName = "Purchase Stock", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },
            [nameof(ItemStockSummaryModel.SaleStock)] = new() { DisplayName = "Sale Stock", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },
            [nameof(ItemStockSummaryModel.MonthlyStock)] = new() { DisplayName = "Monthly Stock", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },
            [nameof(ItemStockSummaryModel.ClosingStock)] = new() { DisplayName = "Closing Stock", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },

            [nameof(ItemStockSummaryModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false, Width = 12 },
            [nameof(ItemStockSummaryModel.ClosingValue)] = new() { DisplayName = "Closing Value", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },

            [nameof(ItemStockSummaryModel.AveragePrice)] = new() { DisplayName = "Average Price", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false, Width = 15 },
            [nameof(ItemStockSummaryModel.WeightedAverageValue)] = new() { DisplayName = "Weighted Avg Value", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 18 },

            [nameof(ItemStockSummaryModel.LastPurchasePrice)] = new() { DisplayName = "Last Purchase Price", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false, Width = 18 },
            [nameof(ItemStockSummaryModel.LastPurchaseValue)] = new() { DisplayName = "Last Purchase Value", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 18 }
        };

        // Define column order based on showAllColumns flag
        List<string> columnOrder;

        // All columns in logical order
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

        // Summary columns only (key information)
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

        MemoryStream stream;

        // If no details data provided, use the simple single-worksheet export
        stream = await ExcelReportExportUtil.ExportToExcel(
            stockData,
            "ITEM STOCK REPORT",
            "Stock Summary",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder
        );

        // Multi-worksheet export
        if (stockDetailsData is not null && stockDetailsData.Any())
            stream = await ExportWithDetails(
                 stockData,
                 stockDetailsData,
                 dateRangeStart,
                 dateRangeEnd,
                 columnSettings,
                 columnOrder
            );

        string fileName = $"ITEM_STOCK_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }

    /// <summary>
    /// Export with both summary and details worksheets
    /// </summary>
    private static async Task<MemoryStream> ExportWithDetails(
        IEnumerable<ItemStockSummaryModel> stockData,
        IEnumerable<ItemStockDetailsModel> stockDetailsData,
        DateOnly? dateRangeStart,
        DateOnly? dateRangeEnd,
        Dictionary<string, ExcelReportExportUtil.ColumnSetting> summaryColumnSettings,
        List<string> summaryColumnOrder)
    {
        // Create the first worksheet with summary data
        var summaryStream = await ExcelReportExportUtil.ExportToExcel(
            stockData,
            "ITEM STOCK REPORT",
            "Stock Summary",
            dateRangeStart,
            dateRangeEnd,
            summaryColumnSettings,
            summaryColumnOrder
        );

        // Define column settings for details worksheet
        var detailsColumnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(ItemStockDetailsModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 10 },
            [nameof(ItemStockDetailsModel.ItemId)] = new() { DisplayName = "Item ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 12 },
            [nameof(ItemStockDetailsModel.TransactionId)] = new() { DisplayName = "Trans ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false, Width = 15 },

            // Text fields
            [nameof(ItemStockDetailsModel.ItemName)] = new() { DisplayName = "Item Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 25 },
            [nameof(ItemStockDetailsModel.ItemCode)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, Width = 15 },
            [nameof(ItemStockDetailsModel.ItemTypeName)] = new() { DisplayName = "Type", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 20 },
            [nameof(ItemStockDetailsModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 20 },
            [nameof(ItemStockDetailsModel.ManufacturerName)] = new() { DisplayName = "Manufacturer", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 20 },
            [nameof(ItemStockDetailsModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, Width = 18 },
            [nameof(ItemStockDetailsModel.Type)] = new() { DisplayName = "Trans Type", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, Width = 18 },

            // Date fields
            [nameof(ItemStockDetailsModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, Width = 15 },
            // Numeric fields
            [nameof(ItemStockDetailsModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true, Width = 15 },
            [nameof(ItemStockDetailsModel.NetRate)] = new() { DisplayName = "Net Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false, Width = 12 }
        };

        // Define column order for details
        var detailsColumnOrder = new List<string>
        {
            nameof(ItemStockDetailsModel.TransactionDateTime),
            nameof(ItemStockDetailsModel.TransactionNo),
            nameof(ItemStockDetailsModel.Type),
            nameof(ItemStockDetailsModel.ItemName),
            nameof(ItemStockDetailsModel.ItemCode),
            nameof(ItemStockDetailsModel.ItemTypeName),
            nameof(ItemStockDetailsModel.ItemCategoryName),
            nameof(ItemStockDetailsModel.ManufacturerName),
            nameof(ItemStockDetailsModel.Quantity),
            nameof(ItemStockDetailsModel.NetRate)
        };

        // Create the details worksheet
        var detailsStream = await ExcelReportExportUtil.ExportToExcel(
            stockDetailsData,
            "ITEM STOCK DETAILS",
            "Transaction Details",
            dateRangeStart,
            dateRangeEnd,
            detailsColumnSettings,
            detailsColumnOrder
        );

        // Now combine both worksheets into one workbook
        return CombineWorksheets(summaryStream, detailsStream);
    }

    /// <summary>
    /// Combine two Excel streams into a single workbook with multiple worksheets
    /// </summary>
    private static MemoryStream CombineWorksheets(MemoryStream summaryStream, MemoryStream detailsStream)
    {
        using var excelEngine = new Syncfusion.XlsIO.ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = Syncfusion.XlsIO.ExcelVersion.Xlsx;

        // Load the summary workbook
        var workbook = application.Workbooks.Open(summaryStream);

        // Load the details workbook
        var detailsWorkbook = application.Workbooks.Open(detailsStream);

        // Copy the worksheet from details workbook to main workbook
        workbook.Worksheets.AddCopy(detailsWorkbook.Worksheets[0]);

        // Close the details workbook
        detailsWorkbook.Close();

        // Save the combined workbook to a new stream
        var combinedStream = new MemoryStream();
        workbook.SaveAs(combinedStream);
        combinedStream.Position = 0;

        // Clean up
        summaryStream.Dispose();
        detailsStream.Dispose();

        return combinedStream;
    }
}
