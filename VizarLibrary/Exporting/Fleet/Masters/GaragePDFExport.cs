using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class GaragePDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<GarageModel> garageData)
    {
        var enrichedData = garageData.Select(garage => new
        {
            garage.Id,
            garage.Name,
            garage.Remarks,
            Status = garage.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(GarageModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(GarageModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(GarageModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(GarageModel.Status)] = new()
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
            nameof(GarageModel.Id),
            nameof(GarageModel.Name),
            nameof(GarageModel.Remarks),
            nameof(GarageModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Garage MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Garage_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
