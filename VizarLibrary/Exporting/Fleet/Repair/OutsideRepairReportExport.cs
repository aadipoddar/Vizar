using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class OutsideRepairReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<OutsideRepairOverviewModel> transactionData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showSummary = false,
		CompanyModel company = null,
		LedgerModel vendor = null,
		VehicleModel vehicle = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(OutsideRepairOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.VendorId)] = new() { DisplayName = "Vendor ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.VendorName)] = new() { DisplayName = "Vendor", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.ApprovedBy)] = new() { DisplayName = "Approved By", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(OutsideRepairOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(OutsideRepairOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(OutsideRepairOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
			[nameof(OutsideRepairOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = CellAlignment.Right },
		};

		List<string> columnOrder;

		if (showSummary)
		{
			columnOrder =
			[
				nameof(OutsideRepairOverviewModel.VehicleCode),
				nameof(OutsideRepairOverviewModel.TotalItems),
				nameof(OutsideRepairOverviewModel.TotalQuantity),
				nameof(OutsideRepairOverviewModel.TotalAmount)
			];

			if (vehicle is not null)
				columnOrder.Remove(nameof(OutsideRepairOverviewModel.VehicleCode));
		}
		else if (showAllColumns)
		{
			columnOrder =
			[
				nameof(OutsideRepairOverviewModel.TransactionNo),
				nameof(OutsideRepairOverviewModel.TransactionDateTime),
				nameof(OutsideRepairOverviewModel.VendorName),
				nameof(OutsideRepairOverviewModel.CompanyName),
				nameof(OutsideRepairOverviewModel.VehicleCode),
				nameof(OutsideRepairOverviewModel.CurrentHour),
				nameof(OutsideRepairOverviewModel.CurrentKM),
				nameof(OutsideRepairOverviewModel.ApprovedBy),
				nameof(OutsideRepairOverviewModel.FinancialYear),
				nameof(OutsideRepairOverviewModel.TotalItems),
				nameof(OutsideRepairOverviewModel.TotalQuantity),
				nameof(OutsideRepairOverviewModel.TotalAmount),
				nameof(OutsideRepairOverviewModel.Remarks),
				nameof(OutsideRepairOverviewModel.CreatedByName),
				nameof(OutsideRepairOverviewModel.CreatedAt),
				nameof(OutsideRepairOverviewModel.CreatedFromPlatform),
				nameof(OutsideRepairOverviewModel.LastModifiedByUserName),
				nameof(OutsideRepairOverviewModel.LastModifiedAt),
				nameof(OutsideRepairOverviewModel.LastModifiedFromPlatform)
			];

			if (company is not null)
				columnOrder.Remove(nameof(OutsideRepairOverviewModel.CompanyName));

			if (vendor is not null)
				columnOrder.Remove(nameof(OutsideRepairOverviewModel.VendorName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(OutsideRepairOverviewModel.VehicleCode));
		}
		else
		{
			columnOrder =
			[
				nameof(OutsideRepairOverviewModel.TransactionNo),
				nameof(OutsideRepairOverviewModel.TransactionDateTime),
				nameof(OutsideRepairOverviewModel.VendorName),
				nameof(OutsideRepairOverviewModel.VehicleCode),
				nameof(OutsideRepairOverviewModel.TotalQuantity),
				nameof(OutsideRepairOverviewModel.TotalAmount)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(OutsideRepairOverviewModel.VendorName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(OutsideRepairOverviewModel.VehicleCode));
		}

		string fileName = $"OUTSIDE_REPAIR_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				transactionData,
				"OUTSIDE REPAIR REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns || showSummary,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				transactionData,
				"OUTSIDE REPAIR REPORT",
				"Outside Repair Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
			);

			return (stream, fileName + ".xlsx");
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportItemReport(
		IEnumerable<OutsideRepairItemOverviewModel> transactionData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showSummary = false,
		CompanyModel company = null,
		LedgerModel vendor = null,
		VehicleModel vehicle = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(OutsideRepairItemOverviewModel.MasterId)] = new() { DisplayName = "Master ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairItemOverviewModel.CompanyId)] = new() { DisplayName = "Company ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairItemOverviewModel.VendorId)] = new() { DisplayName = "Vendor ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairItemOverviewModel.VehicleId)] = new() { DisplayName = "Vehicle ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(OutsideRepairItemOverviewModel.Job)] = new() { DisplayName = "Job", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.VendorName)] = new() { DisplayName = "Vendor", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.ApprovedBy)] = new() { DisplayName = "Approved By", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.Remarks)] = new() { DisplayName = "Job Remarks", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.OutsideRepairRemarks)] = new() { DisplayName = "Repair Remarks", Alignment = CellAlignment.Left },
			[nameof(OutsideRepairItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center },
			[nameof(OutsideRepairItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(OutsideRepairItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
			[nameof(OutsideRepairItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(OutsideRepairItemOverviewModel.CurrentHour)] = new() { DisplayName = "Current Hour", Format = "#,##0.00", Alignment = CellAlignment.Right },
			[nameof(OutsideRepairItemOverviewModel.CurrentKM)] = new() { DisplayName = "Current KM", Format = "#,##0.00", Alignment = CellAlignment.Right }
		};

		List<string> columnOrder;

		if (showSummary)
		{
			columnOrder =
			[
				nameof(OutsideRepairItemOverviewModel.Job),
				nameof(OutsideRepairItemOverviewModel.Quantity),
				nameof(OutsideRepairItemOverviewModel.Total)
			];
		}
		else if (showAllColumns)
		{
			columnOrder =
			[
				nameof(OutsideRepairItemOverviewModel.Job),
				nameof(OutsideRepairItemOverviewModel.TransactionNo),
				nameof(OutsideRepairItemOverviewModel.TransactionDateTime),
				nameof(OutsideRepairItemOverviewModel.CompanyName),
				nameof(OutsideRepairItemOverviewModel.VendorName),
				nameof(OutsideRepairItemOverviewModel.VehicleCode),
				nameof(OutsideRepairItemOverviewModel.CurrentHour),
				nameof(OutsideRepairItemOverviewModel.CurrentKM),
				nameof(OutsideRepairItemOverviewModel.ApprovedBy),
				nameof(OutsideRepairItemOverviewModel.Quantity),
				nameof(OutsideRepairItemOverviewModel.Rate),
				nameof(OutsideRepairItemOverviewModel.Total),
				nameof(OutsideRepairItemOverviewModel.Remarks),
				nameof(OutsideRepairItemOverviewModel.OutsideRepairRemarks)
			];

			if (company is not null)
				columnOrder.Remove(nameof(OutsideRepairItemOverviewModel.CompanyName));

			if (vendor is not null)
				columnOrder.Remove(nameof(OutsideRepairItemOverviewModel.VendorName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(OutsideRepairItemOverviewModel.VehicleCode));
		}
		else
		{
			columnOrder =
			[
				nameof(OutsideRepairItemOverviewModel.Job),
				nameof(OutsideRepairItemOverviewModel.TransactionNo),
				nameof(OutsideRepairItemOverviewModel.TransactionDateTime),
				nameof(OutsideRepairItemOverviewModel.VendorName),
				nameof(OutsideRepairItemOverviewModel.VehicleCode),
				nameof(OutsideRepairItemOverviewModel.Quantity),
				nameof(OutsideRepairItemOverviewModel.Rate),
				nameof(OutsideRepairItemOverviewModel.Total)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(OutsideRepairItemOverviewModel.VendorName));

			if (vehicle is not null)
				columnOrder.Remove(nameof(OutsideRepairItemOverviewModel.VehicleCode));
		}

		string fileName = $"OUTSIDE_REPAIR_ITEM_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				transactionData,
				"OUTSIDE REPAIR ITEM REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns || showSummary,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				transactionData,
				"OUTSIDE REPAIR ITEM REPORT",
				"Outside Repair Items",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Vehicle"] = vehicle?.Code ?? null }
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
