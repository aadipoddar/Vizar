using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class GarageIssueItemReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<GarageIssueItemOverviewModel> transactionData,
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
            [nameof(GarageIssueItemOverviewModel.ItemId)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.ItemCategoryId)] = new() { DisplayName = "Category ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.ItemIssueRemarks)] = new() { DisplayName = "Item Issue Remarks", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left },
            [nameof(GarageIssueItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center },
            [nameof(GarageIssueItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(GarageIssueItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(GarageIssueItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showSummary)
        {
            columnOrder =
            [
                nameof(GarageIssueItemOverviewModel.ItemName),
                nameof(GarageIssueItemOverviewModel.ItemCode),
                nameof(GarageIssueItemOverviewModel.ItemCategoryName),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Total)
            ];
        }
        else if (showAllColumns)
        {
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

            if (garage is not null)
                columnOrder.Remove(nameof(GarageIssueItemOverviewModel.GarageName));

            if (company is not null)
                columnOrder.Remove(nameof(GarageIssueItemOverviewModel.CompanyName));
        }
        else
        {
            columnOrder =
            [
                nameof(GarageIssueItemOverviewModel.ItemName),
                nameof(GarageIssueItemOverviewModel.TransactionNo),
                nameof(GarageIssueItemOverviewModel.TransactionDateTime),
                nameof(GarageIssueItemOverviewModel.GarageName),
                nameof(GarageIssueItemOverviewModel.Quantity),
                nameof(GarageIssueItemOverviewModel.Rate),
                nameof(GarageIssueItemOverviewModel.Total)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(GarageIssueItemOverviewModel.GarageName));
        }

        string fileName = $"GARAGE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "GARAGE ITEM ISSUE REPORT",
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
                "GARAGE ITEM ISSUE REPORT",
                "Garage Item Issue Transactions",
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
