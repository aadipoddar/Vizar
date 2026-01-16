using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

public static class VehicleIssueItemReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<VehicleIssueItemOverviewModel> transactionData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false,
        VehicleModel vehicle = null,
        CompanyModel company = null)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(VehicleIssueItemOverviewModel.ItemId)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.ItemCategoryId)] = new() { DisplayName = "Category ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle Code", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.VehicleShortCode)] = new() { DisplayName = "Vehicle Short", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.ItemIssueRemarks)] = new() { DisplayName = "Item Issue Remarks", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left },
            [nameof(VehicleIssueItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center },
            [nameof(VehicleIssueItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleIssueItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(VehicleIssueItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(VehicleIssueItemOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleIssueItemOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleIssueItemOverviewModel.PreviousHour)] = new() { DisplayName = "Previous Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleIssueItemOverviewModel.PreviousKM)] = new() { DisplayName = "Previous KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(VehicleIssueItemOverviewModel.Average)] = new() { DisplayName = "Average", Format = "#,##0.00", Alignment = CellAlignment.Right }
        };

        List<string> columnOrder;

        if (showSummary)
        {
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

            if (vehicle is not null)
            {
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.VehicleCode));
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.VehicleShortCode));
            }
        }
        else if (showAllColumns)
        {
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

            if (vehicle is not null)
            {
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.VehicleCode));
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.VehicleShortCode));
            }

            if (company is not null)
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.CompanyName));
        }
        else
        {
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
                nameof(VehicleIssueItemOverviewModel.Average)
            ];

            if (vehicle is not null)
            {
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.VehicleCode));
                columnOrder.Remove(nameof(VehicleIssueItemOverviewModel.VehicleShortCode));
            }
        }

        string fileName = $"VEHICLE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "VEHICLE ITEM ISSUE REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                new() { ["Company"] = company?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                transactionData,
                "VEHICLE ITEM ISSUE REPORT",
                "Vehicle Item Issue Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { ["Company"] = company?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
