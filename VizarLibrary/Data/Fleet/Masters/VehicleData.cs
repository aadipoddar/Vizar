using VizarLibrary.DataAccess;
using VizarLibrary.Models.Fleet.Vehicle;

namespace VizarLibrary.Data.Fleet.Vehicle;

public static class VehicleData
{
    public static async Task<int> InsertVehicle(VehicleModel vehicle) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertVehicle, vehicle)).FirstOrDefault();

    public static async Task<int> InsertVehicleType(VehicleTypeModel vehicleType) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertVehicleType, vehicleType)).FirstOrDefault();

    public static async Task<int> InsertVehicleModel(VehicleModelModel vehicleModel) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertVehicleModel, vehicleModel)).FirstOrDefault();
}
