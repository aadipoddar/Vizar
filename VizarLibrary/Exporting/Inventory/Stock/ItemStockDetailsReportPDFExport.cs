using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Stock;

/// <summary>
/// PDF export functionality for Item Stock Details Report
/// </summary>
public static class ItemStockDetailsReportPDFExport
{
    /// <summary>
    /// Export Item Stock Details Report to PDF
    /// </summary>
    /// <param name="stockDetailsData">Collection of item stock details records</param>
    /// <param name="dateRangeStart">Start date of the report</param>
    /// <param name="dateRangeEnd">End date of the report</param>
    /// <returns>MemoryStream containing the PDF file and the file name</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportReport(
        IEnumerable<ItemStockDetailsModel> stockDetailsData,
        DateOnly? dateRangeStart = null,
        DateOnly? dateRangeEnd = null)
    {
        // Define custom column settings for details
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>();

        // Define column order for details (no toggle - always same columns)
        var columnOrder = new List<string>
        {
            nameof(ItemStockDetailsModel.TransactionDateTime),
            nameof(ItemStockDetailsModel.TransactionNo),
            nameof(ItemStockDetailsModel.Type),
            nameof(ItemStockDetailsModel.ItemName),
            nameof(ItemStockDetailsModel.ItemCode),
            nameof(ItemStockDetailsModel.Quantity),
            nameof(ItemStockDetailsModel.NetRate)
        };

        // Customize specific columns for PDF display
        columnSettings[nameof(ItemStockDetailsModel.TransactionDateTime)] = new()
        {
            DisplayName = "Trans Date",
            Format = "dd-MMM-yyyy hh:mm",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockDetailsModel.TransactionNo)] = new()
        {
            DisplayName = "Trans No",
            IncludeInTotal = false
        };

        columnSettings[nameof(ItemStockDetailsModel.Type)] = new()
        {
            DisplayName = "Trans Type",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockDetailsModel.ItemName)] = new()
        {
            DisplayName = "Item Name",
            IncludeInTotal = false
        };

        columnSettings[nameof(ItemStockDetailsModel.ItemCode)] = new()
        {
            DisplayName = "Code",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        columnSettings[nameof(ItemStockDetailsModel.Quantity)] = new()
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

        columnSettings[nameof(ItemStockDetailsModel.NetRate)] = new()
        {
            DisplayName = "Net Rate",
            Format = "#,##0.00",
            IncludeInTotal = false,
            StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
            {
                Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
            }
        };

        // Call the generic PDF export utility with portrait mode
        var stream = await PDFReportExportUtil.ExportToPdf(
            stockDetailsData,
            "ITEM STOCK TRANSACTION DETAILS",
            dateRangeStart,
            dateRangeEnd,
            columnSettings,
            columnOrder,
            useLandscape: false  // Use portrait orientation for details view
        );

        string detailsFileName = $"ITEM_STOCK_DETAILS";
        if (dateRangeStart.HasValue || dateRangeEnd.HasValue)
            detailsFileName += $"_{dateRangeStart?.ToString("yyyyMMdd") ?? "START"}_to_{dateRangeEnd?.ToString("yyyyMMdd") ?? "END"}";
        detailsFileName += ".pdf";

        return (stream, detailsFileName);
    }
}
