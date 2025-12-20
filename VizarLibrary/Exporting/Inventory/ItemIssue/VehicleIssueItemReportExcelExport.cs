using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class VehicleIssueItemReportExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<VehicleIssueItemOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false)
    {
        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(VehicleIssueItemOverviewModel.ItemId)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.ItemCategoryId)] = new() { DisplayName = "Category ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(VehicleIssueItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.VehicleShortCode)] = new() { DisplayName = "Vehicle Short", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.ItemIssueRemarks)] = new() { DisplayName = "Item Issue Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleIssueItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Date fields
            [nameof(VehicleIssueItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Numeric fields - Quantity
            [nameof(VehicleIssueItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleIssueItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleIssueItemOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleIssueItemOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleIssueItemOverviewModel.PreviousHour)] = new() { DisplayName = "Previous Hour", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleIssueItemOverviewModel.PreviousKM)] = new() { DisplayName = "Previous KM", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
        };

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

        // All columns in logical order
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

        // Summary columns only
        else
            columnOrder =
            [
                nameof(VehicleIssueItemOverviewModel.ItemName),
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
                nameof(VehicleIssueItemOverviewModel.Average),
            ];

        // Export using the generic utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "VEHICLE ITEM ISSUE REPORT",
            "Vehicle Item Issue Transactions",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder
        );

        string fileName = $"VEHICLE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }
}
