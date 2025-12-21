using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ItemPDFExport
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

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(ItemModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(ItemModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(ItemModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            ["ItemType"] = new() { DisplayName = "Item Type", IncludeInTotal = false },
            ["ItemCategory"] = new() { DisplayName = "Category", IncludeInTotal = false },
            ["Manufacturer"] = new() { DisplayName = "Manufacturer", IncludeInTotal = false },

            [nameof(ItemModel.Rate)] = new()
            {
                DisplayName = "Rate",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            ["Tax"] = new() { DisplayName = "Tax", IncludeInTotal = false },

            [nameof(ItemModel.UnitOfMeasurement)] = new()
            {
                DisplayName = "Unit",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(ItemModel.ReorderLevel)] = new()
            {
                DisplayName = "Reorder Level",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Right,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(ItemModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(ItemModel.Status)] = new()
            {
                DisplayName = "Status",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            }
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

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Item MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: true
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
