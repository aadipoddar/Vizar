using VizarLibrary.DataAccess;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Data.Fleet.Service;

public static class ServiceScheduleData
{
    public static async Task<int> InsertServiceSchedule(ServiceScheduleModel serviceSchedule) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertServiceSchedule, serviceSchedule)).FirstOrDefault();
}
