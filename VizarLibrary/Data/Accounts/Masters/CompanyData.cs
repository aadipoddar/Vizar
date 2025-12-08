using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Data.Accounts.Masters;

public static class CompanyData
{
    public static async Task<int> InsertCompany(CompanyModel company) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertCompany, company)).FirstOrDefault();
}
