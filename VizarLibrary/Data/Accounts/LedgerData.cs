using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts;

namespace VizarLibrary.Data.Accounts;

public static class LedgerData
{
	public static async Task<int> InsertLedger(LedgerModel ledgerModel) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertLedger, ledgerModel)).FirstOrDefault();
}
