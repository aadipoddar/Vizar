using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class GarageExcelExport
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

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(GarageModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(GarageModel.Name)] = new() { DisplayName = "Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(GarageModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(GarageModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
        };

        var columnOrder = new List<string>
        {
            nameof(GarageModel.Id),
            nameof(GarageModel.Name),
            nameof(GarageModel.Remarks),
            nameof(GarageModel.Status)
        };

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "GARAGE",
            "Garage Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Garage_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
