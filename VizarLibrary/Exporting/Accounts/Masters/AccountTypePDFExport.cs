using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Exporting.Accounts.Masters;

/// <summary>
/// PDF export functionality for Account Type
/// </summary>
public static class AccountTypePDFExport
{
    /// <summary>
    /// Export account type data to PDF with custom column order and formatting
    /// </summary>
    /// <param name="accountTypeData">Collection of account type records</param>
    /// <returns>MemoryStream containing the PDF file</returns>
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<AccountTypeModel> accountTypeData)
    {
        // Create enriched data with status formatting
        var enrichedData = accountTypeData.Select(accountType => new
        {
            accountType.Id,
            accountType.Name,
            accountType.Remarks,
            Status = accountType.Status ? "Active" : "Deleted"
        });

        // Define custom column settings
        var columnSettings = new Dictionary<string, PDFReportExportUtil.ColumnSetting>
        {
            [nameof(AccountTypeModel.Id)] = new()
            {
                DisplayName = "ID",
                StringFormat = new Syncfusion.Pdf.Graphics.PdfStringFormat
                {
                    Alignment = Syncfusion.Pdf.Graphics.PdfTextAlignment.Center,
                    LineAlignment = Syncfusion.Pdf.Graphics.PdfVerticalAlignment.Middle
                },
                IncludeInTotal = false
            },

            [nameof(AccountTypeModel.Name)] = new() { DisplayName = "Account Type Name", IncludeInTotal = false },
            [nameof(AccountTypeModel.Remarks)] = new() { DisplayName = "Remarks", IncludeInTotal = false },

            [nameof(AccountTypeModel.Status)] = new()
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
            nameof(AccountTypeModel.Id),
            nameof(AccountTypeModel.Name),
            nameof(AccountTypeModel.Remarks),
            nameof(AccountTypeModel.Status)
        ];

        // Call the generic PDF export utility
        var stream = await PDFReportExportUtil.ExportToPdf(
            enrichedData,
            "ACCOUNT TYPE MASTER",
            null,
            null,
            columnSettings,
            columnOrder,
            useLandscape: false
        );

        var currentDateTime = CommonData.LoadCurrentDateTime();
        var fileName = $"AccountType_Master_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
        return (stream, fileName);
    }
}
