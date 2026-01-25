using VizarLibrary.DataAccess;
using VizarLibrary.Models.Fleet.Repair;

namespace VizarLibrary.Data.Fleet.Repair;

public static class GarageData
{
    public static async Task<int> InsertGarage(GarageModel garage) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertGarage, garage)).FirstOrDefault();
}
