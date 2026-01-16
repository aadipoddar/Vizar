using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class ItemIssueReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ItemIssueOverviewModel> transactionData,
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
            [nameof(ItemIssueOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemIssueOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(ItemIssueOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(ItemIssueOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(ItemIssueOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center },
            [nameof(ItemIssueOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left },
            [nameof(ItemIssueOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left },
            [nameof(ItemIssueOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(ItemIssueOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center },
            [nameof(ItemIssueOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center },
            [nameof(ItemIssueOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(ItemIssueOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(ItemIssueOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(ItemIssueOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(ItemIssueOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(ItemIssueOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showSummary)
        {
            columnOrder =
            [
                nameof(ItemIssueOverviewModel.GarageName),
                nameof(ItemIssueOverviewModel.TotalItems),
                nameof(ItemIssueOverviewModel.TotalQuantity),
                nameof(ItemIssueOverviewModel.TotalAmount)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(ItemIssueOverviewModel.GarageName));
        }
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(ItemIssueOverviewModel.TransactionNo),
                nameof(ItemIssueOverviewModel.TransactionDateTime),
                nameof(ItemIssueOverviewModel.GarageName),
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

            if (garage is not null)
                columnOrder.Remove(nameof(ItemIssueOverviewModel.GarageName));

            if (company is not null)
                columnOrder.Remove(nameof(ItemIssueOverviewModel.CompanyName));
        }
        else
        {
            columnOrder =
            [
                nameof(ItemIssueOverviewModel.TransactionNo),
                nameof(ItemIssueOverviewModel.TransactionDateTime),
                nameof(ItemIssueOverviewModel.GarageName),
                nameof(ItemIssueOverviewModel.TotalQuantity),
                nameof(ItemIssueOverviewModel.TotalAmount)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(ItemIssueOverviewModel.GarageName));
        }

        string fileName = $"ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "ITEM ISSUE REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                new() { ["Company"] = company?.Name ?? null, ["Garage"] = garage?.Name ?? null }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                transactionData,
                "ITEM ISSUE REPORT",
                "Item Issue Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { ["Company"] = company?.Name ?? null, ["Garage"] = garage?.Name ?? null }
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
