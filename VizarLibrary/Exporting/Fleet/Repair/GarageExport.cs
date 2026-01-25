using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Repair;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class GarageExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
        IEnumerable<GarageModel> garageData,
        ReportExportType exportType)
    {
        var enrichedData = garageData.Select(garage => new
        {
            garage.Id,
            garage.Name,
            External = garage.External ? "Yes" : "No",
            garage.Remarks,
            Status = garage.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(GarageModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
            [nameof(GarageModel.External)] = new() { DisplayName = "External", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(GarageModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(GarageModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
        };

        List<string> columnOrder =
        [
            nameof(GarageModel.Id),
            nameof(GarageModel.Name),
            nameof(GarageModel.External),
            nameof(GarageModel.Remarks),
            nameof(GarageModel.Status)
        ];

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Garage_Master_{currentDateTime:yyyyMMdd_HHmmss}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                enrichedData,
                "GARAGE MASTER",
                null,
                null,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: false
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                enrichedData,
                "GARAGE",
                "Garage Data",
                null,
                null,
                columnSettings,
                columnOrder
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
