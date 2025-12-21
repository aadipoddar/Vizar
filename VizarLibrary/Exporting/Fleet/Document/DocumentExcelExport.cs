using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Document;

namespace VizarLibrary.Exporting.Fleet.Document;

public static class DocumentExcelExport
{
    public static async Task<(MemoryStream stream, string fileName)> ExportMaster(IEnumerable<DocumentOverviewModel> transactionData)
    {
        var columnSettings = new Dictionary<string, ExcelReportExportUtil.ColumnSetting>
        {
            // IDs - Center aligned, no totals
            [nameof(DocumentOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(DocumentOverviewModel.DocumentTypeId)] = new() { DisplayName = "Document Type ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(DocumentOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(DocumentOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },
            [nameof(DocumentOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false },

            // Text fields
            [nameof(DocumentOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(DocumentOverviewModel.DocumentType)] = new() { DisplayName = "Document Type", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(DocumentOverviewModel.Vehicle)] = new() { DisplayName = "Vehicle", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(DocumentOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(DocumentOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(DocumentOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(DocumentOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignLeft },
            [nameof(DocumentOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(DocumentOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Date fields
            [nameof(DocumentOverviewModel.TransactionDateTime)] = new() { DisplayName = "Trans Date", Format = "dd-MMM-yyyy", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(DocumentOverviewModel.RenewalDate)] = new() { DisplayName = "Renewal Date", Format = "dd-MMM-yyyy", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(DocumentOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },
            [nameof(DocumentOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter },

            // Amount fields - All with N2 format and totals
            [nameof(DocumentOverviewModel.CurrentHour)] = new() { DisplayName = "Hour", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(DocumentOverviewModel.CurrentKM)] = new() { DisplayName = "KM", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },
            [nameof(DocumentOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignRight, IncludeInTotal = true, HighlightNegative = true },

            // Status - Center aligned
            [nameof(DocumentOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = Syncfusion.XlsIO.ExcelHAlign.HAlignCenter, IncludeInTotal = false }
        };

        var columnOrder = new List<string>
        {
            nameof(DocumentOverviewModel.TransactionNo),
            nameof(DocumentOverviewModel.TransactionDateTime),
            nameof(DocumentOverviewModel.FinancialYear),
            nameof(DocumentOverviewModel.DocumentType),
            nameof(DocumentOverviewModel.Vehicle),
            nameof(DocumentOverviewModel.CurrentHour),
            nameof(DocumentOverviewModel.CurrentKM),
            nameof(DocumentOverviewModel.Rate),
            nameof(DocumentOverviewModel.RenewalDate),
            nameof(DocumentOverviewModel.Remarks),
            nameof(DocumentOverviewModel.CreatedByName),
            nameof(DocumentOverviewModel.CreatedAt),
            nameof(DocumentOverviewModel.CreatedFromPlatform),
            nameof(DocumentOverviewModel.LastModifiedByUserName),
            nameof(DocumentOverviewModel.LastModifiedAt),
            nameof(DocumentOverviewModel.LastModifiedFromPlatform),
            nameof(DocumentOverviewModel.Status)
        };

        var stream = await ExcelReportExportUtil.ExportToExcel(
            transactionData,
            "DOCUMENT MASTER",
            "Documents",
            null,
            null,
            columnSettings,
            columnOrder
        );

        var currentDateTime = await CommonData.LoadCurrentDateTime();
        var fileName = $"DOCUMENT_Master_{currentDateTime:yyyyMMdd_HHmmss}.xlsx";
        return (stream, fileName);
    }
}
