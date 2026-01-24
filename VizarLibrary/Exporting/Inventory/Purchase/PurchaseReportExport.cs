using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

public static class PurchaseReportExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<PurchaseOverviewModel> purchaseData,
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
            [nameof(PurchaseOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.ReceiveDateTime)] = new() { DisplayName = "Receive Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.VendorName)] = new() { DisplayName = "Vendor", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.TotalItems)] = new() { DisplayName = "Items", Format = "#,##0", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(PurchaseOverviewModel.TotalQuantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true },
            [nameof(PurchaseOverviewModel.BaseTotal)] = new() { DisplayName = "Base Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.ItemDiscountAmount)] = new() { DisplayName = "Dis Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.TotalAfterItemDiscount)] = new() { DisplayName = "After Disc", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.TotalInclusiveTaxAmount)] = new() { DisplayName = "Incl Tax", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.TotalExtraTaxAmount)] = new() { DisplayName = "Extra Tax", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.TotalAfterTax)] = new() { DisplayName = "Sub Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.OtherChargesPercent)] = new() { DisplayName = "Other Charges %", Format = "#,##0.00", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.OtherChargesAmount)] = new() { DisplayName = "Other Charges Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.CashDiscountPercent)] = new() { DisplayName = "Cash Disc %", Format = "#,##0.00", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseOverviewModel.CashDiscountAmount)] = new() { DisplayName = "Cash Disc Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.RoundOffAmount)] = new() { DisplayName = "Round Off", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseOverviewModel.TotalAmount)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showSummary)
        {
            columnOrder =
            [
                nameof(PurchaseOverviewModel.VendorName),
                nameof(PurchaseOverviewModel.GarageName),
                nameof(PurchaseOverviewModel.TotalItems),
                nameof(PurchaseOverviewModel.TotalQuantity),
                nameof(PurchaseOverviewModel.BaseTotal),
                nameof(PurchaseOverviewModel.ItemDiscountAmount),
                nameof(PurchaseOverviewModel.TotalAfterItemDiscount),
                nameof(PurchaseOverviewModel.TotalInclusiveTaxAmount),
                nameof(PurchaseOverviewModel.TotalExtraTaxAmount),
                nameof(PurchaseOverviewModel.TotalAfterTax),
                nameof(PurchaseOverviewModel.CashDiscountAmount),
                nameof(PurchaseOverviewModel.OtherChargesAmount),
                nameof(PurchaseOverviewModel.RoundOffAmount),
                nameof(PurchaseOverviewModel.TotalAmount)
            ];

            if (vendor is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.VendorName));

            if (garage is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.GarageName));
        }

        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(PurchaseOverviewModel.TransactionNo),
                nameof(PurchaseOverviewModel.TransactionDateTime),
                nameof(PurchaseOverviewModel.ReceiveDateTime),
                nameof(PurchaseOverviewModel.VendorName),
                nameof(PurchaseOverviewModel.GarageName),
                nameof(PurchaseOverviewModel.CompanyName),
                nameof(PurchaseOverviewModel.FinancialYear),
                nameof(PurchaseOverviewModel.TotalItems),
                nameof(PurchaseOverviewModel.TotalQuantity),
                nameof(PurchaseOverviewModel.BaseTotal),
                nameof(PurchaseOverviewModel.ItemDiscountAmount),
                nameof(PurchaseOverviewModel.TotalAfterItemDiscount),
                nameof(PurchaseOverviewModel.TotalInclusiveTaxAmount),
                nameof(PurchaseOverviewModel.TotalExtraTaxAmount),
                nameof(PurchaseOverviewModel.TotalAfterTax),
                nameof(PurchaseOverviewModel.OtherChargesPercent),
                nameof(PurchaseOverviewModel.OtherChargesAmount),
                nameof(PurchaseOverviewModel.CashDiscountPercent),
                nameof(PurchaseOverviewModel.CashDiscountAmount),
                nameof(PurchaseOverviewModel.RoundOffAmount),
                nameof(PurchaseOverviewModel.TotalAmount),
                nameof(PurchaseOverviewModel.Remarks),
                nameof(PurchaseOverviewModel.CreatedByName),
                nameof(PurchaseOverviewModel.CreatedAt),
                nameof(PurchaseOverviewModel.CreatedFromPlatform),
                nameof(PurchaseOverviewModel.LastModifiedByUserName),
                nameof(PurchaseOverviewModel.LastModifiedAt),
                nameof(PurchaseOverviewModel.LastModifiedFromPlatform)
            ];

            if (vendor is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.VendorName));

            if (company is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.CompanyName));

            if (garage is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.GarageName));
        }

        else
        {
            columnOrder =
            [
                nameof(PurchaseOverviewModel.VendorName),
                nameof(PurchaseOverviewModel.GarageName),
                nameof(PurchaseOverviewModel.TransactionNo),
                nameof(PurchaseOverviewModel.TransactionDateTime),
                nameof(PurchaseOverviewModel.ReceiveDateTime),
                nameof(PurchaseOverviewModel.TotalQuantity),
                nameof(PurchaseOverviewModel.TotalAfterTax),
                nameof(PurchaseOverviewModel.OtherChargesPercent),
                nameof(PurchaseOverviewModel.CashDiscountPercent),
                nameof(PurchaseOverviewModel.TotalAmount)
            ];

            if (vendor is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.VendorName));

            if (garage is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.GarageName));
        }

        string fileName = $"PURCHASE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                purchaseData,
                "PURCHASE REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                new() { ["Company"] = company?.Name ?? null, ["Party"] = vendor?.Name ?? null }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                purchaseData,
                "PURCHASE REPORT",
                "Purchase Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { ["Company"] = company?.Name ?? null, ["Party"] = vendor?.Name ?? null }
            );

            return (stream, fileName + ".xlsx");
        }
    }

    public static async Task<(MemoryStream stream, string fileName)> ExportItemReport(
        IEnumerable<PurchaseItemOverviewModel> purchaseItemData,
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
            [nameof(PurchaseItemOverviewModel.ItemName)] = new() { DisplayName = "Item", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.ItemCode)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.ItemCategoryName)] = new() { DisplayName = "Category", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.ReceiveDateTime)] = new() { DisplayName = "Receive Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.VendorName)] = new() { DisplayName = "Vendor", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.PurchaseRemarks)] = new() { DisplayName = "Purchase Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.IdentificationNo)] = new() { DisplayName = "Identification", Alignment = CellAlignment.Left, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.InclusiveTax)] = new() { DisplayName = "Incl Tax", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.Quantity)] = new() { DisplayName = "Qty", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.NetRate)] = new() { DisplayName = "Net Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.BaseTotal)] = new() { DisplayName = "Base Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.DiscountPercent)] = new() { DisplayName = "Disc %", Format = "#,##0.00", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.DiscountAmount)] = new() { DisplayName = "Disc Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.AfterDiscount)] = new() { DisplayName = "After Disc", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.SGSTPercent)] = new() { DisplayName = "SGST %", Format = "#,##0.00", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.SGSTAmount)] = new() { DisplayName = "SGST Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.CGSTPercent)] = new() { DisplayName = "CGST %", Format = "#,##0.00", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.CGSTAmount)] = new() { DisplayName = "CGST Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.IGSTPercent)] = new() { DisplayName = "IGST %", Format = "#,##0.00", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(PurchaseItemOverviewModel.IGSTAmount)] = new() { DisplayName = "IGST Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.TotalTaxAmount)] = new() { DisplayName = "Tax Amt", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.Total)] = new() { DisplayName = "Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
            [nameof(PurchaseItemOverviewModel.NetTotal)] = new() { DisplayName = "Net Total", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true }
        };

        List<string> columnOrder;

        if (showSummary)
        {
            columnOrder =
            [
                nameof(PurchaseItemOverviewModel.ItemName),
                nameof(PurchaseItemOverviewModel.ItemCode),
                nameof(PurchaseItemOverviewModel.ItemCategoryName),
                nameof(PurchaseItemOverviewModel.GarageName),
                nameof(PurchaseItemOverviewModel.Quantity),
                nameof(PurchaseItemOverviewModel.BaseTotal),
                nameof(PurchaseItemOverviewModel.DiscountAmount),
                nameof(PurchaseItemOverviewModel.AfterDiscount),
                nameof(PurchaseItemOverviewModel.SGSTAmount),
                nameof(PurchaseItemOverviewModel.CGSTAmount),
                nameof(PurchaseItemOverviewModel.IGSTAmount),
                nameof(PurchaseItemOverviewModel.TotalTaxAmount),
                nameof(PurchaseItemOverviewModel.Total),
                nameof(PurchaseItemOverviewModel.NetTotal)
            ];

            if (garage is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.GarageName));
        }

        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(PurchaseItemOverviewModel.ItemName),
                nameof(PurchaseItemOverviewModel.ItemCode),
                nameof(PurchaseItemOverviewModel.ItemCategoryName),
                nameof(PurchaseItemOverviewModel.TransactionNo),
                nameof(PurchaseItemOverviewModel.TransactionDateTime),
                nameof(PurchaseItemOverviewModel.ReceiveDateTime),
                nameof(PurchaseItemOverviewModel.CompanyName),
                nameof(PurchaseItemOverviewModel.VendorName),
                nameof(PurchaseItemOverviewModel.GarageName),
                nameof(PurchaseItemOverviewModel.IdentificationNo),
                nameof(PurchaseItemOverviewModel.Quantity),
                nameof(PurchaseItemOverviewModel.Rate),
                nameof(PurchaseItemOverviewModel.BaseTotal),
                nameof(PurchaseItemOverviewModel.DiscountPercent),
                nameof(PurchaseItemOverviewModel.DiscountAmount),
                nameof(PurchaseItemOverviewModel.AfterDiscount),
                nameof(PurchaseItemOverviewModel.SGSTPercent),
                nameof(PurchaseItemOverviewModel.SGSTAmount),
                nameof(PurchaseItemOverviewModel.CGSTPercent),
                nameof(PurchaseItemOverviewModel.CGSTAmount),
                nameof(PurchaseItemOverviewModel.IGSTPercent),
                nameof(PurchaseItemOverviewModel.IGSTAmount),
                nameof(PurchaseItemOverviewModel.TotalTaxAmount),
                nameof(PurchaseItemOverviewModel.InclusiveTax),
                nameof(PurchaseItemOverviewModel.Total),
                nameof(PurchaseItemOverviewModel.NetRate),
                nameof(PurchaseItemOverviewModel.NetTotal),
                nameof(PurchaseItemOverviewModel.PurchaseRemarks),
                nameof(PurchaseItemOverviewModel.Remarks)
            ];

            if (vendor is not null)
                columnOrder.Remove(nameof(PurchaseItemOverviewModel.VendorName));

            if (company is not null)
                columnOrder.Remove(nameof(PurchaseItemOverviewModel.CompanyName));

            if (garage is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.GarageName));
        }

        else
        {
            columnOrder =
            [
                nameof(PurchaseItemOverviewModel.ItemName),
                nameof(PurchaseItemOverviewModel.ItemCode),
                nameof(PurchaseItemOverviewModel.TransactionNo),
                nameof(PurchaseItemOverviewModel.TransactionDateTime),
                nameof(PurchaseItemOverviewModel.ReceiveDateTime),
                nameof(PurchaseItemOverviewModel.VendorName),
                nameof(PurchaseItemOverviewModel.GarageName),
                nameof(PurchaseItemOverviewModel.Quantity),
                nameof(PurchaseItemOverviewModel.NetRate),
                nameof(PurchaseItemOverviewModel.NetTotal)
            ];

            if (vendor is not null)
                columnOrder.Remove(nameof(PurchaseItemOverviewModel.VendorName));

            if (garage is not null)
                columnOrder.Remove(nameof(PurchaseOverviewModel.GarageName));
        }

        string fileName = $"PURCHASE_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                purchaseItemData,
                "PURCHASE ITEM REPORT",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: showAllColumns || showSummary,
                new() { ["Company"] = company?.Name ?? null, ["Party"] = vendor?.Name ?? null }
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                purchaseItemData,
                "PURCHASE ITEM REPORT",
                "Purchase Item Transactions",
                dateRangeStart,
                dateRangeEnd,
                columnSettings,
                columnOrder,
                new() { ["Company"] = company?.Name ?? null, ["Party"] = vendor?.Name ?? null }
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
