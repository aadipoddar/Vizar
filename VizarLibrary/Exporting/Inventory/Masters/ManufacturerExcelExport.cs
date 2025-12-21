using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ManufacturerExcelExport
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

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(ManufacturerModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(ManufacturerModel.Name)] = new() { DisplayName = "Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(ManufacturerModel.Code)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(ManufacturerModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(ManufacturerModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
        };

        var columnOrder = new List<string>
        {
            nameof(ManufacturerModel.Id),
            nameof(ManufacturerModel.Name),
            nameof(ManufacturerModel.Code),
            nameof(ManufacturerModel.Remarks),
            nameof(ManufacturerModel.Status)
        };

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "MANUFACTURER",
            "Manufacturer Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Manufacturer_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
