using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehicleModelPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleModelModel> vehicleModelData)
    {
        var manufacturers = await CommonData.LoadTableDataByStatus<ManufacturerModel>(TableNames.Manufacturer);

        var enrichedData = vehicleModelData.Select(vehicleModel => new
        {
            vehicleModel.Id,
            vehicleModel.Name,
            vehicleModel.Code,
            Manufacturer = manufacturers.FirstOrDefault(m => m.Id == vehicleModel.ManufacturerId)?.Name ?? "",
            vehicleModel.Remarks,
            Status = vehicleModel.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(VehicleModelModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(VehicleModelModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(VehicleModelModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            ["Manufacturer"] = new() { DisplayName = "Manufacturer", IncludeInTotal = false },
            [nameof(VehicleModelModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(VehicleModelModel.Status)] = new()
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
            nameof(VehicleModelModel.Id),
            nameof(VehicleModelModel.Name),
            nameof(VehicleModelModel.Code),
            "Manufacturer",
            nameof(VehicleModelModel.Remarks),
            nameof(VehicleModelModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Vehicle Model MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Vehicle_Model_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
