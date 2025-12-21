using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehicleTypePDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleTypeModel> vehicleTypeData)
    {
        var enrichedData = vehicleTypeData.Select(vehicleType => new
        {
            vehicleType.Id,
            vehicleType.Name,
            vehicleType.Code,
            vehicleType.Remarks,
            Status = vehicleType.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(VehicleTypeModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(VehicleTypeModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(VehicleTypeModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(VehicleTypeModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(VehicleTypeModel.Status)] = new()
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
            nameof(VehicleTypeModel.Id),
            nameof(VehicleTypeModel.Name),
            nameof(VehicleTypeModel.Code),
            nameof(VehicleTypeModel.Remarks),
            nameof(VehicleTypeModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Vehicle Type MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Vehicle_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
