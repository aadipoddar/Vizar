using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Document;

namespace VizarLibrary.Exporting.Fleet.Document;

public static class DocumentTypeExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<DocumentTypeModel> documentTypeData,
		ReportExportType exportType)
	{
		var enrichedData = documentTypeData.Select(documentType => new
		{
			documentType.Id,
			documentType.Name,
			documentType.Code,
			documentType.Rate,
			documentType.Remarks,
			Status = documentType.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(DocumentTypeModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(DocumentTypeModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(DocumentTypeModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(DocumentTypeModel.Rate)] = new() { DisplayName = "Rate", Format = "#,##0.00", Alignment = CellAlignment.Right, IncludeInTotal = false },
			[nameof(DocumentTypeModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(DocumentTypeModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(DocumentTypeModel.Id),
			nameof(DocumentTypeModel.Name),
			nameof(DocumentTypeModel.Code),
			nameof(DocumentTypeModel.Rate),
			nameof(DocumentTypeModel.Remarks),
			nameof(DocumentTypeModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Document_Type_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"DOCUMENT TYPE MASTER",
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
				"DOCUMENT TYPE",
				"Document Type Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
