using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Exporting.Fleet.Vehicle;

public static class VehicleModelExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleModelModel> vehicleModelData,
		ReportExportType exportType)
	{
		var manufacturers = await CommonData.LoadTableDataByStatus<ManufacturerModel>(TableNames.Manufacturer);

		var enrichedData = vehicleModelData.Select(vehicleModel => new
		{
			vehicleModel.Id,
			vehicleModel.Name,
			vehicleModel.Code,
			Manufacturer = manufacturers.FirstOrDefault(m => m.Id == vehicleModel.ManufacturerId)?.Name ?? "",
			vehicleModel.Remarks,
			Status = vehicleModel.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleModelModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleModelModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleModelModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			["Manufacturer"] = new() { DisplayName = "Manufacturer", Alignment = CellAlignment.Left },
			[nameof(VehicleModelModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleModelModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleModelModel.Id),
			nameof(VehicleModelModel.Name),
			nameof(VehicleModelModel.Code),
			"Manufacturer",
			nameof(VehicleModelModel.Remarks),
			nameof(VehicleModelModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Model_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE MODEL MASTER",
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
				"VEHICLE MODEL",
				"Vehicle Model Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
