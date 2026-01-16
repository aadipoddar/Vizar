using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Document;

namespace VizarLibrary.Exporting.Fleet.Document;

public static class DocumentExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<DocumentOverviewModel> transactionData,
		ReportExportType exportType)
	{
		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(DocumentOverviewModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(DocumentOverviewModel.DocumentTypeId)] = new() { DisplayName = "Document Type ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(DocumentOverviewModel.FinancialYearId)] = new() { DisplayName = "Financial Year ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(DocumentOverviewModel.CreatedBy)] = new() { DisplayName = "Created By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(DocumentOverviewModel.LastModifiedBy)] = new() { DisplayName = "Modified By ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(DocumentOverviewModel.TransactionNo)] = new() { DisplayName = "Trans No", Alignment = CellAlignment.Left },
			[nameof(DocumentOverviewModel.DocumentType)] = new() { DisplayName = "Document Type", Alignment = CellAlignment.Left },
			[nameof(DocumentOverviewModel.Vehicle)] = new() { DisplayName = "Vehicle", Alignment = CellAlignment.Left },
			[nameof(DocumentOverviewModel.FinancialYear)] = new() { DisplayName = "Financial Year", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.CreatedByName)] = new() { DisplayName = "Created By", Alignment = CellAlignment.Left },
			[nameof(DocumentOverviewModel.LastModifiedByUserName)] = new() { DisplayName = "Modified By", Alignment = CellAlignment.Left },
			[nameof(DocumentOverviewModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(DocumentOverviewModel.CreatedFromPlatform)] = new() { DisplayName = "Created Platform", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.LastModifiedFromPlatform)] = new() { DisplayName = "Modified Platform", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.TransactionDateTime)] = new() { DisplayName = "Renewal Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.RenewalDate)] = new() { DisplayName = "Next Renewal Date", Format = "dd-MMM-yyyy", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.CreatedAt)] = new() { DisplayName = "Created At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.LastModifiedAt)] = new() { DisplayName = "Modified At", Format = "dd-MMM-yyyy hh:mm tt", Alignment = CellAlignment.Center },
			[nameof(DocumentOverviewModel.CurrentHour)] = new() { DisplayName = "Hour", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(DocumentOverviewModel.CurrentKM)] = new() { DisplayName = "KM", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(DocumentOverviewModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = true, HighlightNegative = true },
			[nameof(DocumentOverviewModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
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
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"DOCUMENT_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				transactionData,
				"DOCUMENT MASTER",
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
				transactionData,
				"DOCUMENT MASTER",
				"Documents",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
