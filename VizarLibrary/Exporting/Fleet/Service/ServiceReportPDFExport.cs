using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class ServiceReportPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ServiceOverviewModel> transactionData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null,
        bool showAllColumns = true,
        string garageName = null,
        bool showSummary = false)
    {
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>();

        List<string> columnOrder;

        // Summary view - grouped by party with totals
        if (showSummary)
            columnOrder =
            [
                nameof(ServiceOverviewModel.GarageName),
                nameof(ServiceOverviewModel.TotalItems),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount)
            ];

        // All columns - detailed view (matching Excel export)
        else if (showAllColumns)
        {
            columnOrder =
            [
                nameof(ServiceOverviewModel.TransactionNo),
                nameof(ServiceOverviewModel.TransactionDateTime),
                nameof(ServiceOverviewModel.CompanyName),
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

            // Add party column only if not filtering by party
            if (string.IsNullOrEmpty(garageName))
                columnOrder.Insert(3, nameof(ServiceOverviewModel.GarageName));
        }

        // Summary columns - key fields only (matching Excel export)
        else
        {
            columnOrder =
            [
                nameof(ServiceOverviewModel.TransactionNo),
                nameof(ServiceOverviewModel.TransactionDateTime),
                nameof(ServiceOverviewModel.TotalQuantity),
                nameof(ServiceOverviewModel.TotalAmount)
            ];

            // Add party column only if not filtering by party
            if (string.IsNullOrEmpty(garageName))
                columnOrder.Insert(2, nameof(ServiceOverviewModel.GarageName));
        }

        columnSettings[nameof(ServiceOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy hh:mm tt", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.CompanyName)] = new() { DisplayName = "Company", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.GarageName)] = new() { DisplayName = "Garage", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false };
        columnSettings[nameof(ServiceOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", IncludeInTotal = false };

        columnSettings[nameof(ServiceOverviewModel.TotalItems)] = new()
        {
            DisplayName = "Items",
            Format = "#,##0",
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ServiceOverviewModel.TotalQuantity)] = new()
        {
            DisplayName = "Qty",
            Format = "#,##0.00",
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ServiceOverviewModel.TotalAmount)] = new()
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
            transactionData,
            "SERVICE REPORT",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: showAllColumns || showSummary,  // Use landscape when showing all columns
            headerMetadata: new() { { "Garage", garageName } }
        );

        string fileName = $"SERVICE_REPORT";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            fileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        fileName += ".pdf";

        return (stream, fileName);
    }
}
