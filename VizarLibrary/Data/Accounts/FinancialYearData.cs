using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts;

namespace VizarLibrary.Data.Accounts;

public class FinancialYearData
{
	public static async Task<FinancialYearModel> LoadFinancialYearByDateTime(DateTime TransactionDateTime) =>
		(await SqlDataAccess.LoadData<FinancialYearModel, dynamic>(StoredProcedureNames.LoadFinancialYearByDateTime, new { TransactionDateTime })).FirstOrDefault();
}
