using VizarLibrary.DataAccess;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data.Item;

public static class ItemTypeData
{
	public static async Task<int> InsertItemType(ItemTypeModel itemType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemType, itemType)).FirstOrDefault();
}
