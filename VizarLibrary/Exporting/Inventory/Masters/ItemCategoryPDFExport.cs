using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ItemCategoryPDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<ItemCategoryModel> itemCategoryData)
    {
        // Create enriched data with status formatting
        var enrichedData = itemCategoryData.Select(itemCategory => new
        {
            itemCategory.Id,
            itemCategory.Name,
            itemCategory.Code,
            itemCategory.Remarks,
            Status = itemCategory.Status ? "Active" : "Deleted"
        });

        // Define custom column settings
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(ItemCategoryModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(ItemCategoryModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(ItemCategoryModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(ItemCategoryModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(ItemCategoryModel.Status)] = new()
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

        // Define column order
        List<string> columnOrder =
        [
            nameof(ItemCategoryModel.Id),
            nameof(ItemCategoryModel.Name),
            nameof(ItemCategoryModel.Code),
            nameof(ItemCategoryModel.Remarks),
            nameof(ItemCategoryModel.Status)
        ];

        // Call the generic PDF export utility
        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Item Category MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Category_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
