using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Data.Accounts.Masters;

public static class StateUTData
{
    public static async Task<int> InsertStateUT(StateUTModel state) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertStateUT, state)).FirstOrDefault();
}