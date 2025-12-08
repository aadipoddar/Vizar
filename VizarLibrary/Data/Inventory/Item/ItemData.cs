using VizarLibrary.DataAccess;
using VizarLibrary.Models.Inventory.Item;

namespace VizarLibrary.Data.Inventory.Item;

public static class ItemData
{
	public static async Task<int> InsertItem(ItemModel item) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItem, item)).FirstOrDefault();

	public static async Task<int> InsertItemType(ItemTypeModel itemType) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemType, itemType)).FirstOrDefault();

	public static async Task<int> InsertItemCategory(ItemCategoryModel itemCategory) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemCategory, itemCategory)).FirstOrDefault();

	public static async Task<int> InsertManufacturer(ManufacturerModel manufacturer) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertManufacturer, manufacturer)).FirstOrDefault();

    public static async Task<int> InsertTax(TaxModel tax) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertTax, tax)).FirstOrDefault();
}
