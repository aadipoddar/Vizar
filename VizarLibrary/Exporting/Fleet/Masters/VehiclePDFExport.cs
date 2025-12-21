using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehiclePDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<VehicleModel> vehicleData)
    {
        var vehicleTypes = await CommonData.LoadTableDataByStatus<VehicleTypeModel>(TableNames.VehicleType);
        var vehicleModels = await CommonData.LoadTableDataByStatus<VehicleModelModel>(TableNames.VehicleModel);

        var enrichedData = vehicleData.Select(vehicle => new
        {
            vehicle.Id,
            vehicle.Code,
            vehicle.ShortCode,
            vehicle.ChasisCode,
            VehicleType = vehicleTypes.FirstOrDefault(vt => vt.Id == vehicle.VehicleTypeId)?.Name ?? "",
            VehicleModel = vehicleModels.FirstOrDefault(vm => vm.Id == vehicle.VehicleModelId)?.Name ?? "",
            PurchaseDate = vehicle.PurchaseDate.ToString("dd-MM-yyyy"),
            OpeningHour = vehicle.OpeningHour?.ToString("N2") ?? "",
            OpeningKM = vehicle.OpeningKM?.ToString("N2") ?? "",
            vehicle.Remarks,
            Status = vehicle.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(VehicleModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(VehicleModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(VehicleModel.ShortCode)] = new() { DisplayName = "Short Code", IncludeInTotal = false },
            [nameof(VehicleModel.ChasisCode)] = new() { DisplayName = "Chasis Code", IncludeInTotal = false },
            ["VehicleType"] = new() { DisplayName = "Vehicle Type", IncludeInTotal = false },
            ["VehicleModel"] = new() { DisplayName = "Vehicle Model", IncludeInTotal = false },
            ["PurchaseDate"] = new()
            {
                DisplayName = "Purchase Date",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            ["OpeningHour"] = new()
            {
                DisplayName = "Opening Hour",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            ["OpeningKM"] = new()
            {
                DisplayName = "Opening KM",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },
            [nameof(VehicleModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(VehicleModel.Status)] = new()
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
            nameof(VehicleModel.Id),
            nameof(VehicleModel.Code),
            nameof(VehicleModel.ShortCode),
            nameof(VehicleModel.ChasisCode),
            "VehicleType",
            "VehicleModel",
            "PurchaseDate",
            "OpeningHour",
            "OpeningKM",
            nameof(VehicleModel.Remarks),
            nameof(VehicleModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Vehicle MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: true
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Vehicle_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
