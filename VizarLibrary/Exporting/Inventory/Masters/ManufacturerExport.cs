using VizarLibrary.Data.Common;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Inventory.Masters;

public static class ManufacturerExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<ManufacturerModel> manufacturerData,
		ReportExportType exportType)
	{
		var enrichedData = manufacturerData.Select(manufacturer => new
		{
			manufacturer.Id,
			manufacturer.Name,
			manufacturer.Code,
			manufacturer.Remarks,
			Status = manufacturer.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(ManufacturerModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(ManufacturerModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(ManufacturerModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(ManufacturerModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(ManufacturerModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(ManufacturerModel.Id),
			nameof(ManufacturerModel.Name),
			nameof(ManufacturerModel.Code),
			nameof(ManufacturerModel.Remarks),
			nameof(ManufacturerModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Manufacturer_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"MANUFACTURER MASTER",
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
				"MANUFACTURER",
				"Manufacturer Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
