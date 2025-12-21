using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ItemExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<ItemModel> itemData)
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

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(ItemModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(ItemModel.Name)] = new() { DisplayName = "Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(ItemModel.Code)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            ["ItemType"] = new() { DisplayName = "Item Type", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            ["ItemCategory"] = new() { DisplayName = "Item Category", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            ["Manufacturer"] = new() { DisplayName = "Manufacturer", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Numeric fields - Right aligned
            [nameof(ItemModel.Rate)] = new() { DisplayName = "Rate", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "#,##0.00" },
            ["Tax"] = new() { DisplayName = "Tax", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(ItemModel.UnitOfMeasurement)] = new() { DisplayName = "Unit", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(ItemModel.ReorderLevel)] = new() { DisplayName = "Reorder Level", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "#,##0.00" },
            [nameof(ItemModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(ItemModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
        };

        var columnOrder = new List<string>
        {
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
        };

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "ITEM MASTER",
            "Item Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
