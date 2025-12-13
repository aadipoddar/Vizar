using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class GarageIssueItemReportExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<GarageIssueItemOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false)
    {
        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(GarageIssueItemOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.ItemCategoryId)] = new() { DisplayName = "Category ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(GarageIssueItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.ItemIssueRemarks)] = new() { DisplayName = "Item Issue Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageIssueItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Date fields
            [nameof(GarageIssueItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Numeric fields - Quantity
            [nameof(GarageIssueItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(GarageIssueItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
        };

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
                nameof(GarageIssueItemOverviewModel.Total),
            ];

        // All columns in logical order
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
                nameof(GarageIssueItemOverviewModel.IdentificationNo),
                nameof(GarageIssueItemOverviewModel.UnitOfMeasurement),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Rate),
                nameof(GarageIssueItemOverviewModel.Total),
                nameof(GarageIssueItemOverviewModel.ItemIssueRemarks),
                nameof(GarageIssueItemOverviewModel.Remarks)
            ];

        // Summary columns only
        else
            columnOrder =
            [
                nameof(GarageIssueItemOverviewModel.ItemName),
                nameof(GarageIssueItemOverviewModel.ItemCode),
                nameof(GarageIssueItemOverviewModel.TransactionNo),
                nameof(GarageIssueItemOverviewModel.TransactionDateTime),
                nameof(GarageIssueItemOverviewModel.GarageName),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Rate),
                nameof(GarageIssueItemOverviewModel.Total)
            ];

        // Export using the generic utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "GARAGE ITEM ISSUE REPORT",
            "Garage Item Issue Transactions",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder
        );

        string fileName = $"GARAGE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }
}
