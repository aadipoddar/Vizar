using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class VehicleServiceItemReportPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<VehicleServiceItemOverviewModel> purchaseReturnItemData,
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
                nameof(VehicleServiceItemOverviewModel.VehicleCode),
                nameof(VehicleServiceItemOverviewModel.CurrentHour),
                nameof(VehicleServiceItemOverviewModel.CurrentKM),
                nameof(VehicleServiceItemOverviewModel.Quantity),
                nameof(VehicleServiceItemOverviewModel.Total),
                nameof(VehicleServiceItemOverviewModel.PreviousHour),
                nameof(VehicleServiceItemOverviewModel.PreviousKM),
                nameof(VehicleServiceItemOverviewModel.Average)
            ];

        // All columns in logical order
        else if (showAllColumns)
            columnOrder =
            [
                nameof(VehicleServiceItemOverviewModel.ServiceTypeName),
                nameof(VehicleServiceItemOverviewModel.ServiceTypeCode),
                nameof(VehicleServiceItemOverviewModel.TransactionNo),
                nameof(VehicleServiceItemOverviewModel.TransactionDateTime),
                nameof(VehicleServiceItemOverviewModel.CompanyName),
                nameof(VehicleServiceItemOverviewModel.GarageName),
                nameof(VehicleServiceItemOverviewModel.VehicleCode),
                nameof(VehicleServiceItemOverviewModel.CurrentHour),
                nameof(VehicleServiceItemOverviewModel.CurrentKM),
                nameof(VehicleServiceItemOverviewModel.Quantity),
                nameof(VehicleServiceItemOverviewModel.Rate),
                nameof(VehicleServiceItemOverviewModel.Total),
                nameof(VehicleServiceItemOverviewModel.PreviousHour),
                nameof(VehicleServiceItemOverviewModel.PreviousKM),
                nameof(VehicleServiceItemOverviewModel.Average),
                nameof(VehicleServiceItemOverviewModel.IntervalDays),
                nameof(VehicleServiceItemOverviewModel.NextDueDate),
                nameof(VehicleServiceItemOverviewModel.ServiceRemarks),
                nameof(VehicleServiceItemOverviewModel.Remarks)
            ];

        // Summary columns only
        else
            columnOrder =
            [
                nameof(VehicleServiceItemOverviewModel.ServiceTypeName),
                nameof(VehicleServiceItemOverviewModel.TransactionNo),
                nameof(VehicleServiceItemOverviewModel.TransactionDateTime),
                nameof(VehicleServiceItemOverviewModel.GarageName),
                nameof(VehicleServiceItemOverviewModel.VehicleCode),
                nameof(VehicleServiceItemOverviewModel.CurrentHour),
                nameof(VehicleServiceItemOverviewModel.CurrentKM),
                nameof(VehicleServiceItemOverviewModel.Rate),
                nameof(VehicleServiceItemOverviewModel.Total),
                nameof(VehicleServiceItemOverviewModel.NextDueDate)
            ];

        // Customize specific columns for PDF display (matching Excel column names)
        columnSettings[nameof(VehicleServiceItemOverviewModel.ServiceTypeName)] = new() { DisplayName = "Service", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.ServiceTypeCode)] = new() { DisplayName = "Code", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.GarageName)] = new() { DisplayName = "Garage", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.VehicleCode)] = new() { DisplayName = "Vehicle", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.VehicleShortCode)] = new() { DisplayName = "Vehicle Short", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.ServiceRemarks)] = new() { DisplayName = "Purchase Return Remarks", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.Remarks)] = new() { DisplayName = "Item Remarks", IncludeInTotal = false };
        columnSettings[nameof(VehicleServiceItemOverviewModel.NextDueDate)] = new() { DisplayName = "Next Due", Format = "dd-MMM-yyyy", IncludeInTotal = false };

        columnSettings[nameof(VehicleServiceItemOverviewModel.CurrentHour)] = new()
        {
            DisplayName = "Current Hour",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleServiceItemOverviewModel.CurrentKM)] = new()
        {
            DisplayName = "Current KM",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleServiceItemOverviewModel.PreviousHour)] = new()
        {
            DisplayName = "Previous Hour",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleServiceItemOverviewModel.PreviousKM)] = new()
        {
            DisplayName = "Previous KM",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleServiceItemOverviewModel.Average)] = new()
        {
            DisplayName = "Average",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleServiceItemOverviewModel.IntervalDays)] = new()
        {
            DisplayName = "Interval",
            Format = "#,##0.00",
            HighlightNegative = true,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(VehicleServiceItemOverviewModel.Quantity)] = new()
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

        columnSettings[nameof(VehicleServiceItemOverviewModel.Rate)] = new()
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

        columnSettings[nameof(VehicleServiceItemOverviewModel.Total)] = new()
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

        // Call the generic PDF export utility with landscape mode for all columns
        var stream = await PDFReportExportUtil.ExportToPdf(
            purchaseReturnItemData,
            "VEHICLE ITEM ISSUE REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns || showSummary  // Use landscape when showing all columns
        );

        string fileName = $"VEHICLE_ITEM_ISSUE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
