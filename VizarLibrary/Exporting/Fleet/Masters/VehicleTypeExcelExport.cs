using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehicleTypeExcelExport
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

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(VehicleTypeModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(VehicleTypeModel.Name)] = new() { DisplayName = "Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(VehicleTypeModel.Code)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(VehicleTypeModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(VehicleTypeModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
        };

        var columnOrder = new List<string>
        {
            nameof(VehicleTypeModel.Id),
            nameof(VehicleTypeModel.Name),
            nameof(VehicleTypeModel.Code),
            nameof(VehicleTypeModel.Remarks),
            nameof(VehicleTypeModel.Status)
        };

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "VEHICLE TYPE",
            "Vehicle Type Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Vehicle_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
