using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Item;

namespace VizarLibrary.Exporting.Inventory.Stock;

public static class ItemStockReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportSummaryReport(
        IEnumerable<ItemStockSummaryModel> stockData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(ItemStockSummaryModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.ItemTypeName)] = new() { DisplayName = "Type", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.ManufacturerName)] = new() { DisplayName = "Manufacturer", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.OpeningStock)] = new() { DisplayName = "Opening Stock", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.PurchaseStock)] = new() { DisplayName = "Purchase Stock", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.SaleStock)] = new() { DisplayName = "Sale Stock", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.MonthlyStock)] = new() { DisplayName = "Monthly Stock", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.ClosingStock)] = new() { DisplayName = "Closing Stock", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.ClosingValue)] = new() { DisplayName = "Closing Value", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.AveragePrice)] = new() { DisplayName = "Average Price", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.WeightedAverageValue)] = new() { DisplayName = "Weighted Avg Value", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockSummaryModel.LastPurchasePrice)] = new() { DisplayName = "Last Purchase Price", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(ItemStockSummaryModel.LastPurchaseValue)] = new() { DisplayName = "Last Purchase Value", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showAllColumns)
        {
            columnOrder =
            [
                nameof(ItemStockSummaryModel.ItemName),
                nameof(ItemStockSummaryModel.ItemCode),
                nameof(ItemStockSummaryModel.ItemCategoryName),
                nameof(ItemStockSummaryModel.ItemTypeName),
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
        }
        else
        {
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
        }

        string fileName = $"ITEM_STOCK_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                stockData,
                "ITEM STOCK REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: true
            );

            fileName += ".pdf";
            return (stream, fileName);
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                stockData,
                "ITEM STOCK REPORT",
                "Stock Summary",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder
            );

            fileName += ".xlsx";
            return (stream, fileName);
        }
    }

    public static async Task<(MemoryStream stream, string fileName)> ExportDetailsReport(
        IEnumerable<ItemStockDetailsModel> stockDetailsData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(ItemStockDetailsModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemStockDetailsModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(ItemStockDetailsModel.Type)] = new() { DisplayName = "Trans Type", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemStockDetailsModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(ItemStockDetailsModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemStockDetailsModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(ItemStockDetailsModel.NetRate)] = new() { DisplayName = "Net Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false }
        };

        var columnOrder = new List<string>
        {
            nameof(ItemStockDetailsModel.TransactionDateTime),
            nameof(ItemStockDetailsModel.TransactionNo),
            nameof(ItemStockDetailsModel.Type),
            nameof(ItemStockDetailsModel.ItemName),
            nameof(ItemStockDetailsModel.ItemCode),
            nameof(ItemStockDetailsModel.Quantity),
            nameof(ItemStockDetailsModel.NetRate)
        };

        string fileName = $"ITEM_STOCK_DETAILS";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                stockDetailsData,
                "ITEM STOCK DETAILS",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: false
            );

            fileName += ".pdf";
            return (stream, fileName);
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                stockDetailsData,
                "ITEM STOCK DETAILS",
                "Transaction Details",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder
            );

            fileName += ".xlsx";
            return (stream, fileName);
        }
    }
}
