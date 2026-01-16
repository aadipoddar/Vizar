using VizarLibrary.DataAccess;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Data.Fleet.Masters;

public static class GarageData
{
    public static async Task<int> InsertGarage(GarageModel garage) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertGarage, garage)).FirstOrDefault();
}
