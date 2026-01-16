using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Masters;

public static class VehicleExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<VehicleModel> vehicleData,
		ReportExportType exportType)
	{
		var vehicleTypes = await CommonData.LoadTableDataByStatus<VehicleTypeModel>(TableNames.VehicleType);
		var vehicleModels = await CommonData.LoadTableDataByStatus<VehicleModelModel>(TableNames.VehicleModel);

		var enrichedData = vehicleData.Select(vehicle => new
		{
			vehicle.Id,
			vehicle.Code,
			vehicle.ShortCode,
			vehicle.ChasisCode,
			VehicleType = vehicleTypes.FirstOrDefault(vt => vt.Id == vehicle.VehicleTypeId)?.Name ?? "",
			VehicleModel = vehicleModels.FirstOrDefault(vm => vm.Id == vehicle.VehicleModelId)?.Name ?? "",
			PurchaseDate = vehicle.PurchaseDate.ToString("dd-MM-yyyy"),
			vehicle.OpeningHour,
			vehicle.OpeningKM,
			vehicle.Remarks,
			Status = vehicle.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(VehicleModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(VehicleModel.Code)] = new() { DisplayName = "Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleModel.ShortCode)] = new() { DisplayName = "Short Code", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(VehicleModel.ChasisCode)] = new() { DisplayName = "Chasis Code", Alignment = CellAlignment.Left, IsRequired = true },
			["VehicleType"] = new() { DisplayName = "Type", Alignment = CellAlignment.Left },
			["VehicleModel"] = new() { DisplayName = "Model", Alignment = CellAlignment.Left },
			["PurchaseDate"] = new() { DisplayName = "Purchase Date", Alignment = CellAlignment.Center },
			[nameof(VehicleModel.OpeningHour)] = new() { DisplayName = "Opening Hour", Alignment = CellAlignment.Right, Format = "N2" },
			[nameof(VehicleModel.OpeningKM)] = new() { DisplayName = "Opening KM", Alignment = CellAlignment.Right, Format = "N2" },
			[nameof(VehicleModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(VehicleModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(VehicleModel.Id),
			nameof(VehicleModel.Code),
			nameof(VehicleModel.ShortCode),
			nameof(VehicleModel.ChasisCode),
			"VehicleType",
			"VehicleModel",
			"PurchaseDate",
			nameof(VehicleModel.OpeningHour),
			nameof(VehicleModel.OpeningKM),
			nameof(VehicleModel.Remarks),
			nameof(VehicleModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Vehicle_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"VEHICLE MASTER",
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
				enrichedData,
				"VEHICLE",
				"Vehicle Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
