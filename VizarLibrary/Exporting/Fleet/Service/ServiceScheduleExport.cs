using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Exporting.Fleet.Service;

public static class ServiceScheduleExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<ServiceScheduleModel> serviceScheduleData,
		ReportExportType exportType)
	{
		var serviceTypes = await CommonData.LoadTableData<ServiceTypeModel>(TableNames.ServiceType);
		var vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(TableNames.VehicleType);

		var enrichedData = serviceScheduleData.Select(schedule => new
		{
			schedule.Id,
			ServiceType = serviceTypes.FirstOrDefault(s => s.Id == schedule.ServiceTypeId)?.Name ?? "N/A",
			VehicleType = vehicleTypes.FirstOrDefault(v => v.Id == schedule.VehicleTypeId)?.Name ?? "N/A",
			schedule.IntervalDays,
			Status = schedule.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			["Id"] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			["ServiceType"] = new() { DisplayName = "Service Type", Alignment = CellAlignment.Left, IsRequired = true },
			["VehicleType"] = new() { DisplayName = "Vehicle Type", Alignment = CellAlignment.Left, IsRequired = true },
			["IntervalDays"] = new() { DisplayName = "Interval Days", Format = "#,##0", Alignment = CellAlignment.Center, IncludeInTotal = false },
			["Status"] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			"Id",
			"ServiceType",
			"VehicleType",
			"IntervalDays",
			"Status"
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"Service_Schedule_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"SERVICE SCHEDULE MASTER",
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
				"SERVICE SCHEDULE",
				"Service Schedule Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
