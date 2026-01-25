using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class GarageServiceItemReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<GarageServiceItemOverviewModel> transactionData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false,
        GarageModel garage = null,
        CompanyModel company = null)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(GarageServiceItemOverviewModel.ServiceTypeId)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.ServiceTypeName)] = new() { DisplayName = "Service", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.ServiceTypeCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.ServiceRemarks)] = new() { DisplayName = "Service Remarks", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left },
            [nameof(GarageServiceItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center },
            [nameof(GarageServiceItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(GarageServiceItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(GarageServiceItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showSummary)
            columnOrder =
            [
                nameof(GarageServiceItemOverviewModel.ServiceTypeName),
                nameof(GarageServiceItemOverviewModel.ServiceTypeCode),
                nameof(GarageServiceItemOverviewModel.Quantity),
                nameof(GarageServiceItemOverviewModel.Total)
            ];
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
        if (garage is not null)
            columnOrder.Remove(nameof(GarageServiceItemOverviewModel.GarageName));

        if (company is not null)
            columnOrder.Remove(nameof(GarageServiceItemOverviewModel.CompanyName));

        string fileName = $"GARAGE_SERVICE_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "GARAGE SERVICE ITEM REPORT",
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
                "GARAGE SERVICE ITEM REPORT",
                "Garage Service Item Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
