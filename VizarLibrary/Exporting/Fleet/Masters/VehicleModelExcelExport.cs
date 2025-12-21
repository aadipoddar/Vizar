using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehicleModelExcelExport
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

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(VehicleModelModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(VehicleModelModel.Name)] = new() { DisplayName = "Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(VehicleModelModel.Code)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            ["Manufacturer"] = new() { DisplayName = "Manufacturer", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(VehicleModelModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(VehicleModelModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
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

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "VEHICLE MODEL",
            "Vehicle Model Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Vehicle_Model_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
