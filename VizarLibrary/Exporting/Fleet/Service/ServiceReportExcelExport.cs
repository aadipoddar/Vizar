using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class ServiceReportExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ServiceOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        string garageName = null,
        bool showSummary = false)
    {
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(ServiceOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(ServiceOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ServiceOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ServiceOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ServiceOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ServiceOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ServiceOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ServiceOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ServiceOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ServiceOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Date fields
            [nameof(ServiceOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ServiceOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ServiceOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Numeric fields - Items and Quantities
            [nameof(ServiceOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true },
            [nameof(ServiceOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true },

            // Amount fields - All with N2 format and totals
            [nameof(ServiceOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
        };

        // Define column order based on showAllColumns flag
        List<string> columnOrder;

        // Summary view - grouped by party with totals
        if (showSummary)
            columnOrder =
            [
                nameof(ServiceOverviewModel.GarageName),
                nameof(ServiceOverviewModel.TotalItems),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount)
            ];

        // All columns in logical order
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(ServiceOverviewModel.TransactionNo),
                nameof(ServiceOverviewModel.TransactionDateTime),
                nameof(ServiceOverviewModel.CompanyName),
                nameof(ServiceOverviewModel.FinancialYear),
                nameof(ServiceOverviewModel.TotalItems),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount),
                nameof(ServiceOverviewModel.Remarks),
                nameof(ServiceOverviewModel.CreatedByName),
                nameof(ServiceOverviewModel.CreatedAt),
                nameof(ServiceOverviewModel.CreatedFromPlatform),
                nameof(ServiceOverviewModel.LastModifiedByUserName),
                nameof(ServiceOverviewModel.LastModifiedAt),
                nameof(ServiceOverviewModel.LastModifiedFromPlatform)
            ];

            // Add party column only if not filtering by party
            if (string.IsNullOrEmpty(garageName))
                columnOrder.Insert(3, nameof(ServiceOverviewModel.GarageName));
        }

        // Summary columns only
        else
        {
            columnOrder =
            [
                nameof(ServiceOverviewModel.TransactionNo),
                nameof(ServiceOverviewModel.TransactionDateTime),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount)
            ];

            // Add party column only if not filtering by party
            if (string.IsNullOrEmpty(garageName))
                columnOrder.Insert(2, nameof(ServiceOverviewModel.GarageName));
        }

        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "SERVICE REPORT",
            "Service Transactions",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            new() { { "Garage", garageName ?? "All Garages" } }
        );

        var fileName = $"SERVICE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }
}
