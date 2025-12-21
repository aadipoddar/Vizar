using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ItemTypePDFExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<ItemTypeModel> itemTypeData)
    {
        var enrichedData = itemTypeData.Select(itemType => new
        {
            itemType.Id,
            itemType.Name,
            itemType.Code,
            itemType.Remarks,
            Status = itemType.Status ? "Active" : "Deleted"
        });

        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(ItemTypeModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(ItemTypeModel.Name)] = new() { DisplayName = "Name", IncludeInTotal = false },
            [nameof(ItemTypeModel.Code)] = new() { DisplayName = "Code", IncludeInTotal = false },
            [nameof(ItemTypeModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(ItemTypeModel.Status)] = new()
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
            nameof(ItemTypeModel.Id),
            nameof(ItemTypeModel.Name),
            nameof(ItemTypeModel.Code),
            nameof(ItemTypeModel.Remarks),
            nameof(ItemTypeModel.Status)
        };

        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "Item Type MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Item_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
