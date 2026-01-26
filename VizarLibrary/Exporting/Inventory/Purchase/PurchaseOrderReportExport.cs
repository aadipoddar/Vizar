using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

public static class PurchaseOrderReportExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportReport(
		IEnumerable<PurchaseOrderOverviewModel> purchaseOrderData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showSummary = false,
		GarageModel garage = null,
		LedgerModel vendor = null,
		CompanyModel company = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(PurchaseOrderOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.VendorName)] = new() { DisplayName = "Vendor", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.PurchaseTransactionNo)] = new() { DisplayName = "Purchase Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.PurchaseDateTime)] = new() { DisplayName = "Purchase Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.PurchaseReceiveDateTime)] = new() { DisplayName = "Purchase Receive Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = CellAlignment.Right, IncludeInTotal = true },
			[nameof(PurchaseOrderOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true }
		};

		List<string> columnOrder;

		if (showSummary)
		{
			columnOrder =
			[
				nameof(PurchaseOrderOverviewModel.VendorName),
				nameof(PurchaseOrderOverviewModel.GarageName),
				nameof(PurchaseOrderOverviewModel.TotalItems),
				nameof(PurchaseOrderOverviewModel.TotalQuantity)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.VendorName));

			if (garage is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.GarageName));
		}

		else if (showAllColumns)
		{
			columnOrder =
			[
				nameof(PurchaseOrderOverviewModel.TransactionNo),
				nameof(PurchaseOrderOverviewModel.TransactionDateTime),
				nameof(PurchaseOrderOverviewModel.VendorName),
				nameof(PurchaseOrderOverviewModel.GarageName),
				nameof(PurchaseOrderOverviewModel.CompanyName),
				nameof(PurchaseOrderOverviewModel.PurchaseTransactionNo),
				nameof(PurchaseOrderOverviewModel.PurchaseDateTime),
				nameof(PurchaseOrderOverviewModel.PurchaseReceiveDateTime),
				nameof(PurchaseOrderOverviewModel.FinancialYear),
				nameof(PurchaseOrderOverviewModel.TotalItems),
				nameof(PurchaseOrderOverviewModel.TotalQuantity),
				nameof(PurchaseOrderOverviewModel.Remarks),
				nameof(PurchaseOrderOverviewModel.CreatedByName),
				nameof(PurchaseOrderOverviewModel.CreatedAt),
				nameof(PurchaseOrderOverviewModel.CreatedFromPlatform),
				nameof(PurchaseOrderOverviewModel.LastModifiedByUserName),
				nameof(PurchaseOrderOverviewModel.LastModifiedAt),
				nameof(PurchaseOrderOverviewModel.LastModifiedFromPlatform)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.VendorName));

			if (company is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.CompanyName));

			if (garage is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.GarageName));
		}

		else
		{
			columnOrder =
			[
				nameof(PurchaseOrderOverviewModel.VendorName),
				nameof(PurchaseOrderOverviewModel.GarageName),
				nameof(PurchaseOrderOverviewModel.TransactionNo),
				nameof(PurchaseOrderOverviewModel.TransactionDateTime),
				nameof(PurchaseOrderOverviewModel.PurchaseTransactionNo),
				nameof(PurchaseOrderOverviewModel.TotalItems),
				nameof(PurchaseOrderOverviewModel.TotalQuantity)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.VendorName));

			if (garage is not null)
				columnOrder.Remove(nameof(PurchaseOrderOverviewModel.GarageName));
		}

		string fileName = $"PURCHASE_ORDER_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				purchaseOrderData,
				"PURCHASE ORDER REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns || showSummary,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Garage"] = garage?.Name ?? null }
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				purchaseOrderData,
				"PURCHASE ORDER REPORT",
				"Purchase Order Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Garage"] = garage?.Name ?? null }
			);

			return (stream, fileName + ".xlsx");
		}
	}

	public static async Task<(MemoryStream stream, string fileName)> ExportItemReport(
		IEnumerable<PurchaseOrderItemOverviewModel> purchaseOrderItemData,
		ReportExportType exportType,
		DateOnly? dateRangeStart = null,
		DateOnly? dateRangeEnd = null,
		bool showAllColumns = true,
		bool showSummary = false,
		GarageModel garage = null,
		LedgerModel vendor = null,
		CompanyModel company = null)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(PurchaseOrderItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.ManufacturerName)] = new() { DisplayName = "Manufacturer", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.VendorName)] = new() { DisplayName = "Vendor", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.PurchaseTransactionNo)] = new() { DisplayName = "Purchase Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.PurchaseDateTime)] = new() { DisplayName = "Purchase Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.PurchaseReceiveDateTime)] = new() { DisplayName = "Purchase Receive Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.PurchaseQuantity)] = new() { DisplayName = "Purchase Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(PurchaseOrderItemOverviewModel.PurchaseOrderRemarks)] = new() { DisplayName = "Order Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.UnitOfMeasurement)] = new() { DisplayName = "UOM", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(PurchaseOrderItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
		};

		List<string> columnOrder;

		if (showSummary)
		{
			columnOrder =
			[
				nameof(PurchaseOrderItemOverviewModel.ItemName),
				nameof(PurchaseOrderItemOverviewModel.ItemCode),
				nameof(PurchaseOrderItemOverviewModel.ItemCategoryName),
				nameof(PurchaseOrderItemOverviewModel.GarageName),
				nameof(PurchaseOrderItemOverviewModel.Quantity),
				nameof(PurchaseOrderItemOverviewModel.PurchaseQuantity)
			];

			if (garage is not null)
				columnOrder.Remove(nameof(PurchaseOrderItemOverviewModel.GarageName));
		}

		else if (showAllColumns)
		{
			columnOrder =
			[
				nameof(PurchaseOrderItemOverviewModel.ItemName),
				nameof(PurchaseOrderItemOverviewModel.ItemCode),
				nameof(PurchaseOrderItemOverviewModel.ItemCategoryName),
				nameof(PurchaseOrderItemOverviewModel.ManufacturerName),
				nameof(PurchaseOrderItemOverviewModel.TransactionNo),
				nameof(PurchaseOrderItemOverviewModel.TransactionDateTime),
				nameof(PurchaseOrderItemOverviewModel.CompanyName),
				nameof(PurchaseOrderItemOverviewModel.VendorName),
				nameof(PurchaseOrderItemOverviewModel.GarageName),
				nameof(PurchaseOrderItemOverviewModel.PurchaseTransactionNo),
				nameof(PurchaseOrderItemOverviewModel.PurchaseDateTime),
				nameof(PurchaseOrderItemOverviewModel.PurchaseReceiveDateTime),
				nameof(PurchaseOrderItemOverviewModel.UnitOfMeasurement),
				nameof(PurchaseOrderItemOverviewModel.Quantity),
				nameof(PurchaseOrderItemOverviewModel.PurchaseQuantity),
				nameof(PurchaseOrderItemOverviewModel.PurchaseOrderRemarks),
				nameof(PurchaseOrderItemOverviewModel.Remarks)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(PurchaseOrderItemOverviewModel.VendorName));

			if (company is not null)
				columnOrder.Remove(nameof(PurchaseOrderItemOverviewModel.CompanyName));

			if (garage is not null)
				columnOrder.Remove(nameof(PurchaseOrderItemOverviewModel.GarageName));
		}

		else
		{
			columnOrder =
			[
				nameof(PurchaseOrderItemOverviewModel.ItemName),
				nameof(PurchaseOrderItemOverviewModel.ItemCode),
				nameof(PurchaseOrderItemOverviewModel.TransactionNo),
				nameof(PurchaseOrderItemOverviewModel.TransactionDateTime),
				nameof(PurchaseOrderItemOverviewModel.VendorName),
				nameof(PurchaseOrderItemOverviewModel.GarageName),
				nameof(PurchaseOrderItemOverviewModel.PurchaseTransactionNo),
				nameof(PurchaseOrderItemOverviewModel.UnitOfMeasurement),
				nameof(PurchaseOrderItemOverviewModel.Quantity)
			];

			if (vendor is not null)
				columnOrder.Remove(nameof(PurchaseOrderItemOverviewModel.VendorName));

			if (garage is not null)
				columnOrder.Remove(nameof(PurchaseOrderItemOverviewModel.GarageName));
		}

		string fileName = $"PURCHASE_ORDER_ITEM_REPORT";
		if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
			fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				purchaseOrderItemData,
				"PURCHASE ORDER ITEM REPORT",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: showAllColumns || showSummary,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Garage"] = garage?.Name ?? null }
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				purchaseOrderItemData,
				"PURCHASE ORDER ITEM REPORT",
				"Purchase Order Item Transactions",
				dateRangeStart,
				dateRangeEnd,
				columnSettings,
				columnOrder,
				new() { ["Company"] = company?.Name ?? null, ["Vendor"] = vendor?.Name ?? null, ["Garage"] = garage?.Name ?? null }
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
