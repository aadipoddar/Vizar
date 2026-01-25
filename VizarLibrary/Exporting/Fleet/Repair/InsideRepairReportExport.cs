using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class InsideRepairReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<InsideRepairOverviewModel> transactionData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false,
        CompanyModel company = null,
        GarageModel garage = null,
        VehicleModel vehicle = null)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(InsideRepairOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center },
            [nameof(InsideRepairOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(InsideRepairOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center },
            [nameof(InsideRepairOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center },
            [nameof(InsideRepairOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(InsideRepairOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(InsideRepairOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
            [nameof(InsideRepairOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(InsideRepairOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(InsideRepairOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(InsideRepairOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(InsideRepairOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
        };

        List<string> columnOrder;

        if (showSummary)
        {
            columnOrder =
            [
                nameof(InsideRepairOverviewModel.VehicleCode),
                nameof(InsideRepairOverviewModel.TotalItems),
                nameof(InsideRepairOverviewModel.TotalQuantity),
                nameof(InsideRepairOverviewModel.TotalAmount)
            ];

            if (vehicle is not null)
                columnOrder.Remove(nameof(InsideRepairOverviewModel.VehicleCode));
        }
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(InsideRepairOverviewModel.TransactionNo),
                nameof(InsideRepairOverviewModel.TransactionDateTime),
                nameof(InsideRepairOverviewModel.GarageName),
                nameof(InsideRepairOverviewModel.CompanyName),
                nameof(InsideRepairOverviewModel.VehicleCode),
                nameof(InsideRepairOverviewModel.CurrentHour),
                nameof(InsideRepairOverviewModel.CurrentKM),
                nameof(InsideRepairOverviewModel.FinancialYear),
                nameof(InsideRepairOverviewModel.TotalItems),
                nameof(InsideRepairOverviewModel.TotalQuantity),
                nameof(InsideRepairOverviewModel.TotalAmount),
                nameof(InsideRepairOverviewModel.Remarks),
                nameof(InsideRepairOverviewModel.CreatedByName),
                nameof(InsideRepairOverviewModel.CreatedAt),
                nameof(InsideRepairOverviewModel.CreatedFromPlatform),
                nameof(InsideRepairOverviewModel.LastModifiedByUserName),
                nameof(InsideRepairOverviewModel.LastModifiedAt),
                nameof(InsideRepairOverviewModel.LastModifiedFromPlatform)
            ];

            if (company is not null)
                columnOrder.Remove(nameof(InsideRepairOverviewModel.CompanyName));

            if (garage is not null)
                columnOrder.Remove(nameof(InsideRepairOverviewModel.GarageName));

            if (vehicle is not null)
                columnOrder.Remove(nameof(InsideRepairOverviewModel.VehicleCode));
        }
        else
        {
            columnOrder =
            [
                nameof(InsideRepairOverviewModel.TransactionNo),
                nameof(InsideRepairOverviewModel.TransactionDateTime),
                nameof(InsideRepairOverviewModel.GarageName),
                nameof(InsideRepairOverviewModel.VehicleCode),
                nameof(InsideRepairOverviewModel.TotalQuantity),
                nameof(InsideRepairOverviewModel.TotalAmount)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(InsideRepairOverviewModel.GarageName));

            if (vehicle is not null)
                columnOrder.Remove(nameof(InsideRepairOverviewModel.VehicleCode));
        }

        string fileName = $"INSIDE_REPAIR_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "INSIDE REPAIR REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                new() { ["Company"] = company?.Name ?? null, ["Garage"] = garage?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                transactionData,
                "INSIDE REPAIR REPORT",
                "Inside Repair Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { ["Company"] = company?.Name ?? null, ["Garage"] = garage?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
            );

            return (stream, fileName + ".xlsx");
        }
    }

    public static async Task<(MemoryStream stream, string fileName)> ExportItemReport(
        IEnumerable<InsideRepairItemOverviewModel> transactionData,
        ReportExportType exportType,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false,
        CompanyModel company = null,
        GarageModel garage = null,
        VehicleModel vehicle = null)
    {
        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(InsideRepairItemOverviewModel.ItemId)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.ItemCategoryId)] = new() { DisplayName = "Category ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.GarageId)] = new() { DisplayName = "Garage ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left },
            [nameof(InsideRepairItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center },
            [nameof(InsideRepairItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(InsideRepairItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(InsideRepairItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(InsideRepairItemOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(InsideRepairItemOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(InsideRepairItemOverviewModel.PreviousHour)] = new() { DisplayName = "Previous Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(InsideRepairItemOverviewModel.PreviousKM)] = new() { DisplayName = "Previous KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
            [nameof(InsideRepairItemOverviewModel.Average)] = new() { DisplayName = "Average", Format = "#,##0.00", Alignment = CellAlignment.Right }
        };

        List<string> columnOrder;

        if (showSummary)
        {
            columnOrder =
            [
                nameof(InsideRepairItemOverviewModel.ItemName),
                nameof(InsideRepairItemOverviewModel.ItemCode),
                nameof(InsideRepairItemOverviewModel.ItemCategoryName),
                nameof(InsideRepairItemOverviewModel.Quantity),
                nameof(InsideRepairItemOverviewModel.Total)
            ];
        }
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(InsideRepairItemOverviewModel.ItemName),
                nameof(InsideRepairItemOverviewModel.ItemCode),
                nameof(InsideRepairItemOverviewModel.ItemCategoryName),
                nameof(InsideRepairItemOverviewModel.TransactionNo),
                nameof(InsideRepairItemOverviewModel.TransactionDateTime),
                nameof(InsideRepairItemOverviewModel.CompanyName),
                nameof(InsideRepairItemOverviewModel.GarageName),
                nameof(InsideRepairItemOverviewModel.VehicleCode),
                nameof(InsideRepairItemOverviewModel.CurrentHour),
                nameof(InsideRepairItemOverviewModel.CurrentKM),
                nameof(InsideRepairItemOverviewModel.IdentificationNo),
                nameof(InsideRepairItemOverviewModel.UnitOfMeasurement),
                nameof(InsideRepairItemOverviewModel.Quantity),
                nameof(InsideRepairItemOverviewModel.Rate),
                nameof(InsideRepairItemOverviewModel.Total),
                nameof(InsideRepairItemOverviewModel.PreviousHour),
                nameof(InsideRepairItemOverviewModel.PreviousKM),
                nameof(InsideRepairItemOverviewModel.Average),
                nameof(InsideRepairItemOverviewModel.InsideRepairRemarks),
                nameof(InsideRepairItemOverviewModel.Remarks)
            ];

            if (company is not null)
                columnOrder.Remove(nameof(InsideRepairItemOverviewModel.CompanyName));

            if (garage is not null)
                columnOrder.Remove(nameof(InsideRepairItemOverviewModel.GarageName));

            if (vehicle is not null)
                columnOrder.Remove(nameof(InsideRepairItemOverviewModel.VehicleCode));
        }
        else
        {
            columnOrder =
            [
                nameof(InsideRepairItemOverviewModel.ItemName),
                nameof(InsideRepairItemOverviewModel.TransactionNo),
                nameof(InsideRepairItemOverviewModel.TransactionDateTime),
                nameof(InsideRepairItemOverviewModel.GarageName),
                nameof(InsideRepairItemOverviewModel.VehicleCode),
                nameof(InsideRepairItemOverviewModel.CurrentHour),
                nameof(InsideRepairItemOverviewModel.CurrentKM),
                nameof(InsideRepairItemOverviewModel.Quantity),
                nameof(InsideRepairItemOverviewModel.Rate),
                nameof(InsideRepairItemOverviewModel.Total),
                nameof(InsideRepairItemOverviewModel.PreviousHour),
                nameof(InsideRepairItemOverviewModel.PreviousKM),
                nameof(InsideRepairItemOverviewModel.Average)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(InsideRepairItemOverviewModel.GarageName));

            if (vehicle is not null)
                columnOrder.Remove(nameof(InsideRepairItemOverviewModel.VehicleCode));
        }

        string fileName = $"INSIDE_REPAIR_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                transactionData,
                "INSIDE REPAIR ITEM REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                new() { ["Company"] = company?.Name ?? null, ["Garage"] = garage?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                transactionData,
                "INSIDE REPAIR ITEM REPORT",
                "Inside Repair Item Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { ["Company"] = company?.Name ?? null, ["Garage"] = garage?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
