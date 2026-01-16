using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ItemExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
        IEnumerable<ItemModel> itemData,
        ReportExportType exportType)
    {
        var itemTypes = await CommonData.LoadTableDataByStatus<ItemTypeModel>(TableNames.ItemType);
        var itemCategories = await CommonData.LoadTableDataByStatus<ItemCategoryModel>(TableNames.ItemCategory);
        var manufacturers = await CommonData.LoadTableDataByStatus<ManufacturerModel>(TableNames.Manufacturer);
        var taxes = await CommonData.LoadTableDataByStatus<TaxModel>(TableNames.Tax);

        var enrichedData = itemData.Select(item => new
        {
            item.Id,
            item.Name,
            item.Code,
            ItemType = itemTypes.FirstOrDefault(t => t.Id == item.ItemTypeId)?.Name ?? "N/A",
            ItemCategory = itemCategories.FirstOrDefault(c => c.Id == item.ItemCategoryId)?.Name ?? "N/A",
            Manufacturer = manufacturers.FirstOrDefault(m => m.Id == item.ManufacturerId)?.Name ?? "N/A",
            item.Rate,
            Tax = taxes.FirstOrDefault(t => t.Id == item.TaxId)?.Name ?? "N/A",
            item.UnitOfMeasurement,
            item.ReorderLevel,
            item.Remarks,
            Status = item.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, ReportColumnSetting>
        {
            [nameof(ItemModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
            [nameof(ItemModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
            [nameof(ItemModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
            ["ItemType"] = new() { DisplayName = "Type", Alignment = CellAlignment.Left },
            ["ItemCategory"] = new() { DisplayName = "Category", Alignment = CellAlignment.Left },
            ["Manufacturer"] = new() { DisplayName = "Manufacturer", Alignment = CellAlignment.Left },
            [nameof(ItemModel.Rate)] = new() { DisplayName = "Rate", Alignment = CellAlignment.Right, Format = "#,##0.00" },
            ["Tax"] = new() { DisplayName = "Tax", Alignment = CellAlignment.Left },
            [nameof(ItemModel.UnitOfMeasurement)] = new() { DisplayName = "Unit", Alignment = CellAlignment.Center },
            [nameof(ItemModel.ReorderLevel)] = new() { DisplayName = "Reorder Level", Alignment = CellAlignment.Right, Format = "#,##0.00" },
            [nameof(ItemModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
            [nameof(ItemModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
        };

        List<string> columnOrder =
        [
            nameof(ItemModel.Id),
            nameof(ItemModel.Name),
            nameof(ItemModel.Code),
            "ItemType",
            "ItemCategory",
            "Manufacturer",
            nameof(ItemModel.Rate),
            "Tax",
            nameof(ItemModel.UnitOfMeasurement),
            nameof(ItemModel.ReorderLevel),
            nameof(ItemModel.Remarks),
            nameof(ItemModel.Status)
        ];

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Master_{currentDateTime:yyyyMMdd_HHmmss}";

        if (exportType == ReportExportType.PDF)
        {
            var stream = await PDFReportExportUtil.ExportToPdf(
                enrichedData,
                "ITEM MASTER",
                null,
                null,
                columnSettings,
                columnOrder,
                useBuiltInStyle: false,
                useLandscape: true
            );

            return (stream, fileName + ".pdf");
        }
        else
        {
            var stream = await ExcelReportExportUtil.ExportToExcel(
                enrichedData,
                "ITEM MASTER",
                "Item Data",
                null,
                null,
                columnSettings,
                columnOrder
            );

            return (stream, fileName + ".xlsx");
        }
    }
}
