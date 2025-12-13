using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class ItemIssueReportExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ItemIssueOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        string garageName = null,
        bool showSummary = false)
    {
        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(ItemIssueOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(ItemIssueOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemIssueOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemIssueOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemIssueOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ItemIssueOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemIssueOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemIssueOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemIssueOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ItemIssueOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Date fields
            [nameof(ItemIssueOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ItemIssueOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ItemIssueOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Numeric fields - Items and Quantities
            [nameof(ItemIssueOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true },
            [nameof(ItemIssueOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true },

            // Amount fields - All with N2 format and totals
            [nameof(ItemIssueOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
        };

        // Define column order based on showAllColumns flag
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

        // All columns in logical order
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

        // Summary columns only
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

        // Export using the generic utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "ITEM ISSUE REPORT",
            "Item Issue Transactions",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            new() { { "Garage", garageName ?? "All Garages" } }
        );

        var fileName = $"ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }
}
