using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class TaxPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<TaxModel> taxData)
    {
        var enrichedData = taxData.Select(tax => new
        {
            tax.Id,
            tax.Name,
            tax.Code,
            tax.CGST,
            tax.SGST,
            tax.IGST,
            Inclusive = tax.Inclusive ? "Yes" : "No",
            Extra = tax.Extra ? "Yes" : "No",
            tax.Remarks,
            Status = tax.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(TaxModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(TaxModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(TaxModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            
            [nameof(TaxModel.CGST)] = new()
            {
                DisplayName = "CGST %",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            
            [nameof(TaxModel.SGST)] = new()
            {
                DisplayName = "SGST %",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            
            [nameof(TaxModel.IGST)] = new()
            {
                DisplayName = "IGST %",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            
            [nameof(TaxModel.Inclusive)] = new()
            {
                DisplayName = "Inclusive",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            
            [nameof(TaxModel.Extra)] = new()
            {
                DisplayName = "Extra",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            
            [nameof(TaxModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(TaxModel.Status)] = new()
            {
                DisplayName = "Status",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            }
        };

        var columnOrder = new List<string>
        {
            nameof(TaxModel.Id),
            nameof(TaxModel.Name),
            nameof(TaxModel.Code),
            nameof(TaxModel.CGST),
            nameof(TaxModel.SGST),
            nameof(TaxModel.IGST),
            nameof(TaxModel.Inclusive),
            nameof(TaxModel.Extra),
            nameof(TaxModel.Remarks),
            nameof(TaxModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Tax MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Tax_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
