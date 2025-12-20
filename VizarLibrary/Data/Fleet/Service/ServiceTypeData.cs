using VizarLibrary.DataAccess;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Data.Fleet.Service;

public static class ServiceTypeData
{
    public static async Task<int> InsertServiceType(ServiceTypeModel serviceType) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertServiceType, serviceType)).FirstOrDefault();
}
