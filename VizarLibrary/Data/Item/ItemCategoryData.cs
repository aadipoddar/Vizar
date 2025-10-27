using VizarLibrary.DataAccess;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data.Item;

public static class ItemCategoryData
{
	public static async Task<int> InsertItemCategory(ItemCategoryModel itemCategory) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemCategory, itemCategory)).FirstOrDefault();
}
