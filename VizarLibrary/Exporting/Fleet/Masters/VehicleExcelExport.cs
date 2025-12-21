using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehicleExcelExport
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
            vehicle.OpeningHour,
            vehicle.OpeningKM,
            vehicle.Remarks,
            Status = vehicle.Status ? "Active" : "Deleted"
        });

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(VehicleModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(VehicleModel.Code)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(VehicleModel.ShortCode)] = new() { DisplayName = "Short Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(VehicleModel.ChasisCode)] = new() { DisplayName = "Chasis Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            ["VehicleType"] = new() { DisplayName = "Vehicle Type", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            ["VehicleModel"] = new() { DisplayName = "Vehicle Model", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            ["PurchaseDate"] = new() { DisplayName = "Purchase Date", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(VehicleModel.OpeningHour)] = new() { DisplayName = "Opening Hour", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "N2" },
            [nameof(VehicleModel.OpeningKM)] = new() { DisplayName = "Opening KM", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "N2" },
            [nameof(VehicleModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(VehicleModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
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
            nameof(VehicleModel.OpeningHour),
            nameof(VehicleModel.OpeningKM),
            nameof(VehicleModel.Remarks),
            nameof(VehicleModel.Status)
        };

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "VEHICLE",
            "Vehicle Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Vehicle_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
