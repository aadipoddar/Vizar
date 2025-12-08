using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Data.Accounts.Masters;

public static class LedgerData
{
    public static async Task<int> InsertLedger(LedgerModel ledger) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertLedger, ledger)).FirstOrDefault();
}
