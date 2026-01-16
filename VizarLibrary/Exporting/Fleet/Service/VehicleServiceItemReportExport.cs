using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class VehicleServiceItemReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<VehicleServiceItemOverviewModel> transactionData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false,
        CompanyModel company = null,
        VehicleModel vehicle = null)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(VehicleServiceItemOverviewModel.ServiceTypeId)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.ServiceTypeName)] = new() { DisplayName = "Service", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.ServiceTypeCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.VehicleShortCode)] = new() { DisplayName = "Vehicle Short", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.ServiceRemarks)] = new() { DisplayName = "Service Remarks", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left },
            [nameof(VehicleServiceItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center },
            [nameof(VehicleServiceItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleServiceItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(VehicleServiceItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleServiceItemOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleServiceItemOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleServiceItemOverviewModel.PreviousHour)] = new() { DisplayName = "Previous Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleServiceItemOverviewModel.PreviousKM)] = new() { DisplayName = "Previous KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleServiceItemOverviewModel.Average)] = new() { DisplayName = "Average", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleServiceItemOverviewModel.IntervalDays)] = new() { DisplayName = "Interval", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleServiceItemOverviewModel.NextDueDate)] = new() { DisplayName = "Next Due", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center }
        };

        List<string> columnOrder;

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
        else if (showAllColumns)
        {
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

            if (company is not null)
                columnOrder.Remove(nameof(VehicleServiceItemOverviewModel.CompanyName));

            if (vehicle is not null)
                columnOrder.Remove(nameof(VehicleServiceItemOverviewModel.VehicleCode));
        }
        else
        {
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

            if (vehicle is not null)
                columnOrder.Remove(nameof(VehicleServiceItemOverviewModel.VehicleCode));
        }

        string fileName = $"VEHICLE_SERVICE_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "VEHICLE SERVICE ITEM REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                transactionData,
                "VEHICLE SERVICE ITEM REPORT",
                "Vehicle Service Item Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
