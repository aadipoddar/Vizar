using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;

namespace VizarLibrary.Data.Common;

public static class UserData
{
	public static async Task InsertUser(UserModel userModel) =>
			await SqlDataAccess.SaveData(StoredProcedureNames.InsertUser, userModel);
}