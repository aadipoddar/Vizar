using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Item;

public static class ItemTypeExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
        IEnumerable<ItemTypeModel> itemTypeData,
        ReportExportType exportType)
    {
        var enrichedData = itemTypeData.Select(itemType => new
        {
            itemType.Id,
            itemType.Name,
            itemType.Code,
            itemType.Remarks,
            Status = itemType.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(ItemTypeModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemTypeModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
            [nameof(ItemTypeModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
            [nameof(ItemTypeModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(ItemTypeModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
        };

        List<string> columnOrder =
        [
            nameof(ItemTypeModel.Id),
            nameof(ItemTypeModel.Name),
            nameof(ItemTypeModel.Code),
            nameof(ItemTypeModel.Remarks),
            nameof(ItemTypeModel.Status)
        ];

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                enrichedData,
                "ITEM TYPE MASTER",
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
                "ITEM TYPE",
                "Item Type Data",
                null,
                null,
                columnSettings,
                columnOrder
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
