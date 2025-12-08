using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Data.Accounts.Masters;

public static class GroupData
{
    public static async Task<int> InsertGroup(GroupModel group) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertGroup, group)).FirstOrDefault();
}
