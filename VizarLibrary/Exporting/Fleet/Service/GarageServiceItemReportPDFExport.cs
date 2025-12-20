using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class GarageServiceItemReportPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<GarageServiceItemOverviewModel> transactionItemData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        bool showSummary = false)
    {
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>();
        List<string> columnOrder;

        // Summary mode - grouped by item with aggregated values
        if (showSummary)
            columnOrder =
            [
                nameof(GarageServiceItemOverviewModel.ServiceTypeName),
                nameof(GarageServiceItemOverviewModel.ServiceTypeCode),
                nameof(GarageServiceItemOverviewModel.Quantity),
                nameof(GarageServiceItemOverviewModel.Total)
            ];

        // All columns - detailed view (matching Excel export)
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
                nameof(GarageServiceItemOverviewModel.Total),
                nameof(GarageServiceItemOverviewModel.ServiceRemarks),
                nameof(GarageServiceItemOverviewModel.Remarks)
            ];

        // Summary columns - key fields only (matching Excel export)
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

        columnSettings[nameof(GarageServiceItemOverviewModel.ServiceTypeName)] = new() { DisplayName = "Serivce", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.ServiceTypeCode)] = new() { DisplayName = "Code", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.ServiceRemarks)] = new() { DisplayName = "Purchase Return Remarks", IncludeInTotal = false };
        columnSettings[nameof(GarageServiceItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", IncludeInTotal = false };

        columnSettings[nameof(GarageServiceItemOverviewModel.Quantity)] = new()
        {
            DisplayName = "Qty",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(GarageServiceItemOverviewModel.Rate)] = new()
        {
            DisplayName = "Rate",
            Format = "#,##0.00",
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            },
            IncludeInTotal = false
        };

        columnSettings[nameof(GarageServiceItemOverviewModel.Total)] = new()
        {
            DisplayName = "Total",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            transactionItemData,
            "GARAGE SERVICE ITEM REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns || showSummary  // Use landscape when showing all columns
        );

        string fileName = $"GARAGE_SERVICE_ITEM_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
