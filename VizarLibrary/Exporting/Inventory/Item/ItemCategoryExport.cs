using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Item;

public static class ItemCategoryExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
        IEnumerable<ItemCategoryModel> itemCategoryData,
        ReportExportType exportType)
    {
        var enrichedData = itemCategoryData.Select(itemCategory => new
        {
            itemCategory.Id,
            itemCategory.Name,
            itemCategory.Code,
            itemCategory.Remarks,
            Status = itemCategory.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(ItemCategoryModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemCategoryModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
            [nameof(ItemCategoryModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
            [nameof(ItemCategoryModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(ItemCategoryModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
        };

        List<string> columnOrder =
        [
            nameof(ItemCategoryModel.Id),
            nameof(ItemCategoryModel.Name),
            nameof(ItemCategoryModel.Code),
            nameof(ItemCategoryModel.Remarks),
            nameof(ItemCategoryModel.Status)
        ];

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Category_Master_{currentDateTime:yyyyMMdd_HHmmss}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                enrichedData,
                "ITEM CATEGORY MASTER",
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
                "ITEM CATEGORY",
                "Item Category Data",
                null,
                null,
                columnSettings,
                columnOrder
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
