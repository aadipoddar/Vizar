using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class GarageServiceItemReportExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<GarageServiceItemOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false)
    {
        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(GarageServiceItemOverviewModel.ServiceTypeId)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(GarageServiceItemOverviewModel.ServiceTypeName)] = new() { DisplayName = "Service", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.ServiceTypeCode)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.ServiceRemarks)] = new() { DisplayName = "Service Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(GarageServiceItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Date fields
            [nameof(GarageServiceItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Numeric fields - Quantity
            [nameof(GarageServiceItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(GarageServiceItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
        };

        // Define column order based on showAllColumns and showSummary flags
        List<string> columnOrder;

        // Summary mode - grouped by item with aggregated values
        if (showSummary)
            columnOrder =
            [
                nameof(GarageServiceItemOverviewModel.ServiceTypeName),
                nameof(GarageServiceItemOverviewModel.ServiceTypeCode),
                nameof(GarageServiceItemOverviewModel.Quantity),
                nameof(GarageServiceItemOverviewModel.Total),
            ];

        // All columns in logical order
        else if (showAllColumns)
            columnOrder =
            [
                nameof(GarageServiceItemOverviewModel.ServiceTypeName),
                nameof(GarageServiceItemOverviewModel.ServiceTypeCode),
                nameof(GarageServiceItemOverviewModel.TransactionNo),
                nameof(GarageServiceItemOverviewModel.TransactionDateTime),
                nameof(GarageServiceItemOverviewModel.CompanyName),
                nameof(GarageServiceItemOverviewModel.GarageName),
                nameof(GarageServiceItemOverviewModel.VehicleCode),
                nameof(GarageServiceItemOverviewModel.Quantity),
                nameof(GarageServiceItemOverviewModel.Rate),
                nameof(GarageServiceItemOverviewModel.Total),
                nameof(GarageServiceItemOverviewModel.Remarks)
            ];

        // Summary columns only
        else
            columnOrder =
            [
                nameof(GarageServiceItemOverviewModel.ServiceTypeName),
                nameof(GarageServiceItemOverviewModel.TransactionNo),
                nameof(GarageServiceItemOverviewModel.TransactionDateTime),
                nameof(GarageServiceItemOverviewModel.GarageName),
                nameof(GarageServiceItemOverviewModel.VehicleCode),
                nameof(GarageServiceItemOverviewModel.Quantity),
                nameof(GarageServiceItemOverviewModel.Rate),
                nameof(GarageServiceItemOverviewModel.Total)
            ];

        // Export using the generic utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "GARAGE SERVICE ITEM REPORT",
            "Garage Service Item Transactions",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder
        );

        string fileName = $"GARAGE_SERVICE_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }
}
