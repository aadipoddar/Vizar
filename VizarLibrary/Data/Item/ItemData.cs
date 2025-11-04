using VizarLibrary.DataAccess;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data.Item;

public static class ItemData
{
	public static async Task<int> InsertItem(ItemModel item) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItem, item)).FirstOrDefault();
}
