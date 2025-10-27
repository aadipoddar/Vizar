using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace VizarLibrary.Data.Common;

public static class UserData
{
	public static async Task<int> InsertUser(UserModel userModel) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertUser, userModel)).FirstOrDefault();
}