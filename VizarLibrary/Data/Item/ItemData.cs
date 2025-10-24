using VizarLibrary.DataAccess;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data.Item;

public static class ItemData
{
	public static async Task<List<ItemModel>> LoadItemByPartyPurchaseDateTime(int PartyId, DateTime PurchaseDateTime, bool OnlyActive = true) =>
		await SqlDataAccess.LoadData<ItemModel, dynamic>(StoredProcedureNames.LoadItemByPartyPurchaseDateTime, new { PartyId, PurchaseDateTime, OnlyActive });
}
