using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class TaxExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<TaxModel> taxData)
    {
        var enrichedData = taxData.Select(tax => new
        {
            tax.Id,
            tax.Name,
            tax.Code,
            tax.CGST,
            tax.SGST,
            tax.IGST,
            Inclusive = tax.Inclusive ? "Yes" : "No",
            Extra = tax.Extra ? "Yes" : "No",
            tax.Remarks,
            Status = tax.Status ? "Active" : "Deleted"
        });

        // Define custom column settings
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // ID - Center aligned, no totals
            [nameof(TaxModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields - Left aligned
            [nameof(TaxModel.Name)] = new() { DisplayName = "Name", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            [nameof(TaxModel.Code)] = new() { DisplayName = "Code", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft, IsRequired = true },
            
            // Numeric fields - Right aligned
            [nameof(TaxModel.CGST)] = new() { DisplayName = "CGST %", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "#,##0.00" },
            [nameof(TaxModel.SGST)] = new() { DisplayName = "SGST %", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "#,##0.00" },
            [nameof(TaxModel.IGST)] = new() { DisplayName = "IGST %", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, Format = "#,##0.00" },
            
            // Boolean fields - Center aligned
            [nameof(TaxModel.Inclusive)] = new() { DisplayName = "Inclusive", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(TaxModel.Extra)] = new() { DisplayName = "Extra", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            
            [nameof(TaxModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },

            // Status - Center aligned
            [nameof(TaxModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
        };

        List<string> columnOrder = new()
        {
            nameof(TaxModel.Id),
            nameof(TaxModel.Name),
            nameof(TaxModel.Code),
            nameof(TaxModel.CGST),
            nameof(TaxModel.SGST),
            nameof(TaxModel.IGST),
            nameof(TaxModel.Inclusive),
            nameof(TaxModel.Extra),
            nameof(TaxModel.Remarks),
            nameof(TaxModel.Status)
        };

        // Call the generic Excel export utility
        var stream = await ExcelReportExportUtil.ExportToExcel(
            enrichedData,
            "TAX MASTER",
            "Tax Data",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"Tax_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
