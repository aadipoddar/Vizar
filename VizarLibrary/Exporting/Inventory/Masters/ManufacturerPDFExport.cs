using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ManufacturerPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<ManufacturerModel> manufacturerData)
    {
        var enrichedData = manufacturerData.Select(manufacturer => new
        {
            manufacturer.Id,
            manufacturer.Name,
            manufacturer.Code,
            manufacturer.Remarks,
            Status = manufacturer.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(ManufacturerModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(ManufacturerModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(ManufacturerModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(ManufacturerModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(ManufacturerModel.Status)] = new()
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
            nameof(ManufacturerModel.Id),
            nameof(ManufacturerModel.Name),
            nameof(ManufacturerModel.Code),
            nameof(ManufacturerModel.Remarks),
            nameof(ManufacturerModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Manufacturer MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Manufacturer_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
