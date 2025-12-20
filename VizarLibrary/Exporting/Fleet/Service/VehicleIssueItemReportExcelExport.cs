using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class VehicleServiceItemReportExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<VehicleServiceItemOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false)
    {
        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(VehicleServiceItemOverviewModel.ServiceTypeId)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(VehicleServiceItemOverviewModel.ServiceTypeName)] = new() { DisplayName = "Service", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.ServiceTypeCode)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.VehicleShortCode)] = new() { DisplayName = "Vehicle Short", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.ServiceRemarks)] = new() { DisplayName = "Service Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleServiceItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Date fields
            [nameof(VehicleServiceItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Numeric fields - Quantity
            [nameof(VehicleServiceItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleServiceItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleServiceItemOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleServiceItemOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleServiceItemOverviewModel.PreviousHour)] = new() { DisplayName = "Previous Hour", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleServiceItemOverviewModel.PreviousKM)] = new() { DisplayName = "Previous KM", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleServiceItemOverviewModel.IntervalDays)] = new() { DisplayName = "Interval", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight },
            [nameof(VehicleServiceItemOverviewModel.NextDueDate)] = new() { DisplayName = "Next Due", Format = "dd-MMM-yyyy", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
        };

        // Define column order based on showAllColumns and showSummary flags
        List<string> columnOrder;

        // Summary mode - grouped by item with aggregated values
        if (showSummary)
            columnOrder =
            [
                nameof(VehicleServiceItemOverviewModel.VehicleCode),
                nameof(VehicleServiceItemOverviewModel.CurrentHour),
                nameof(VehicleServiceItemOverviewModel.CurrentKM),
                nameof(VehicleServiceItemOverviewModel.Quantity),
                nameof(VehicleServiceItemOverviewModel.Total),
                nameof(VehicleServiceItemOverviewModel.PreviousHour),
                nameof(VehicleServiceItemOverviewModel.PreviousKM),
                nameof(VehicleServiceItemOverviewModel.Average)
            ];

        // All columns in logical order
        else if (showAllColumns)
            columnOrder =
            [
                nameof(VehicleServiceItemOverviewModel.ServiceTypeName),
                nameof(VehicleServiceItemOverviewModel.ServiceTypeCode),
                nameof(VehicleServiceItemOverviewModel.TransactionNo),
                nameof(VehicleServiceItemOverviewModel.TransactionDateTime),
                nameof(VehicleServiceItemOverviewModel.CompanyName),
                nameof(VehicleServiceItemOverviewModel.GarageName),
                nameof(VehicleServiceItemOverviewModel.VehicleCode),
                nameof(VehicleServiceItemOverviewModel.CurrentHour),
                nameof(VehicleServiceItemOverviewModel.CurrentKM),
                nameof(VehicleServiceItemOverviewModel.Quantity),
                nameof(VehicleServiceItemOverviewModel.Rate),
                nameof(VehicleServiceItemOverviewModel.Total),
                nameof(VehicleServiceItemOverviewModel.PreviousHour),
                nameof(VehicleServiceItemOverviewModel.PreviousKM),
                nameof(VehicleServiceItemOverviewModel.Average),
                nameof(VehicleServiceItemOverviewModel.IntervalDays),
                nameof(VehicleServiceItemOverviewModel.NextDueDate),
                nameof(VehicleServiceItemOverviewModel.ServiceRemarks),
                nameof(VehicleServiceItemOverviewModel.Remarks)
            ];

        // Summary columns only
        else
            columnOrder =
            [
                nameof(VehicleServiceItemOverviewModel.ServiceTypeName),
                nameof(VehicleServiceItemOverviewModel.TransactionNo),
                nameof(VehicleServiceItemOverviewModel.TransactionDateTime),
                nameof(VehicleServiceItemOverviewModel.GarageName),
                nameof(VehicleServiceItemOverviewModel.VehicleCode),
                nameof(VehicleServiceItemOverviewModel.CurrentHour),
                nameof(VehicleServiceItemOverviewModel.CurrentKM),
                nameof(VehicleServiceItemOverviewModel.Rate),
                nameof(VehicleServiceItemOverviewModel.Total),
                nameof(VehicleServiceItemOverviewModel.NextDueDate)
            ];

        // Export using the generic utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "VEHICLE SERVICE ITEM REPORT",
            "Vehicle Service Item Transactions",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder
        );

        string fileName = $"VEHICLE_SERVICE_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".xlsx";

        return (stream, fileName);
    }
}
