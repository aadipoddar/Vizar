using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class ServiceReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ServiceOverviewModel> transactionData,
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
            [nameof(ServiceOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ServiceOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(ServiceOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(ServiceOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(ServiceOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center },
            [nameof(ServiceOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left },
            [nameof(ServiceOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left },
            [nameof(ServiceOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(ServiceOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center },
            [nameof(ServiceOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center },
            [nameof(ServiceOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(ServiceOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(ServiceOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(ServiceOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(ServiceOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(ServiceOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showSummary)
            columnOrder =
            [
                nameof(ServiceOverviewModel.GarageName),
                nameof(ServiceOverviewModel.TotalItems),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount)
            ];
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(ServiceOverviewModel.TransactionNo),
                nameof(ServiceOverviewModel.TransactionDateTime),
                nameof(ServiceOverviewModel.CompanyName),
                nameof(ServiceOverviewModel.GarageName),
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

            if (garage is not null)
                columnOrder.Remove(nameof(ServiceOverviewModel.GarageName));

            if (company is not null)
                columnOrder.Remove(nameof(ServiceOverviewModel.CompanyName));
        }
        else
        {
            columnOrder =
            [
                nameof(ServiceOverviewModel.TransactionNo),
                nameof(ServiceOverviewModel.TransactionDateTime),
                nameof(ServiceOverviewModel.GarageName),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(ServiceOverviewModel.GarageName));
        }

        string fileName = $"SERVICE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "SERVICE REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                headerMetadata: new() { { "Garage", garage?.Name } }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                transactionData,
                "SERVICE REPORT",
                "Service Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { { "Garage", garage?.Name ?? "All Garages" } }
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
