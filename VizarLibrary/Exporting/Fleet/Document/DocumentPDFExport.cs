using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Document;

namespace VizarLibrary.Exporting.Fleet.Document;

public static class DocumentPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<DocumentOverviewModel> transactionData)
    {
        var columnOrder = new List<string>
        {
            nameof(DocumentOverviewModel.TransactionNo),
            nameof(DocumentOverviewModel.TransactionDateTime),
            nameof(DocumentOverviewModel.DocumentType),
            nameof(DocumentOverviewModel.Vehicle),
            nameof(DocumentOverviewModel.Rate),
            nameof(DocumentOverviewModel.RenewalDate),
            nameof(DocumentOverviewModel.Status)
        };

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(DocumentOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.TransactionDateTime)] = new() { DisplayName = "Renewal", Format = "dd-MMM-yyyy", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.DocumentType)] = new() { DisplayName = "Document Type", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.Vehicle)] = new() { DisplayName = "Vehicle", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.RenewalDate)] = new() { DisplayName = "Next Renewal", Format = "dd-MMM-yyyy", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm", IncludeInTotal = false },
            [nameof(DocumentOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", IncludeInTotal = false },

            [nameof(DocumentOverviewModel.Status)] = new()
            {
                DisplayName = "Status",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(DocumentOverviewModel.CurrentHour)] = new()
            {
                DisplayName = "Hour",
                Format = "#,##0",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                }
            },

            [nameof(DocumentOverviewModel.CurrentKM)] = new()
            {
                DisplayName = "KM",
                Format = "#,##0.00",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                }
            },

            [nameof(DocumentOverviewModel.Rate)] = new()
            {
                DisplayName = "Rate",
                Format = "#,##0.00",
                HighlightNegative = true,
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                }
            }
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
             transactionData,
             "DOCUMENT MASTER",
             null,
             null,
             columnSettings,
             columnOrder,
             useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"DOCUMENT_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
