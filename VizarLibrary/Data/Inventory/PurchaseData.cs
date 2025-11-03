using VizarLibrary.Data.Common;
using VizarLibrary.Data.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data.Inventory;

public static class PurchaseData
{
	public static async Task<int> InsertPurchase(PurchaseModel purchase) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchase, purchase)).FirstOrDefault();

	public static async Task<int> InsertPurchaseDetail(PurchaseDetailModel purchaseDetail) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseDetail, purchaseDetail)).FirstOrDefault();

	public static async Task<List<PurchaseDetailModel>> LoadPurchaseDetailByPurchase(int PurchaseId) =>
		await SqlDataAccess.LoadData<PurchaseDetailModel, dynamic>(StoredProcedureNames.LoadPurchaseDetailByPurchase, new { PurchaseId });

	public static async Task<List<PurchaseOverviewModel>> LoadPurchaseOverviewByDate(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<PurchaseOverviewModel, dynamic>(StoredProcedureNames.LoadPurchaseOverviewByDate, new { StartDate, EndDate });

	public static async Task DeletePurchase(int purchaseId)
	{
		var purchase = await CommonData.LoadTableDataById<PurchaseModel>(TableNames.Purchase, purchaseId);
		if (purchase is not null)
		{
			purchase.Status = false;
			await InsertPurchase(purchase);
		}
	}

	public static async Task<int> SavePurchaseTransaction(PurchaseModel purchase, List<PurchaseItemCartModel> purchaseDetails)
	{
		bool update = purchase.Id > 0;

		purchase.Id = await InsertPurchase(purchase);
		await SavePurchaseDetail(purchase, purchaseDetails, update);
		await UpdateItemRateAndUOMOnPurchase(purchaseDetails);

		return purchase.Id;
	}

	private static async Task SavePurchaseDetail(PurchaseModel purchase, List<PurchaseItemCartModel> purchaseDetails, bool update)
	{
		if (update)
		{
			var existingPurchaseDetails = await LoadPurchaseDetailByPurchase(purchase.Id);
			foreach (var item in existingPurchaseDetails)
			{
				item.Status = false;
				await InsertPurchaseDetail(item);
			}
		}

		foreach (var item in purchaseDetails)
			await InsertPurchaseDetail(new()
			{
				Id = 0,
				PurchaseId = purchase.Id,
				ItemId = item.ItemId,
				IdentificationNo = item.IdentificationNo,
				Quantity = item.Quantity,
				UnitOfMeasurement = item.UnitOfMeasurement,
				Rate = item.Rate,
				BaseTotal = item.BaseTotal,
				DiscountPercent = item.DiscountPercent,
				DiscountAmount = item.DiscountAmount,
				AfterDiscount = item.AfterDiscount,
				CGSTPercent = item.CGSTPercent,
				CGSTAmount = item.CGSTAmount,
				SGSTPercent = item.SGSTPercent,
				SGSTAmount = item.SGSTAmount,
				IGSTPercent = item.IGSTPercent,
				IGSTAmount = item.IGSTAmount,
				TotalTaxAmount = item.TotalTaxAmount,
				InclusiveTax = item.InclusiveTax,
				NetRate = item.NetRate,
				Total = item.Total,
				Remarks = item.Remarks,
				Status = true
			});
	}

	private static async Task UpdateItemRateAndUOMOnPurchase(List<PurchaseItemCartModel> purchaseDetails)
	{
		var isUpdateItemRateOnPurchaseEnabled = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.UpdateItemMasterRateOnPurchase)).Value);
		var isUpdateItemUOMOnPurchaseEnabled = bool.Parse((await SettingsData.LoadSettingsByKey(SettingsKeys.UpdateItemMasterUOMOnPurchase)).Value);

		var items = await CommonData.LoadTableData<ItemModel>(TableNames.Item);

		foreach (var purchaseItem in purchaseDetails)
		{
			var item = items.FirstOrDefault(i => i.Id == purchaseItem.ItemId);
			if (item is not null)
			{
				if (isUpdateItemRateOnPurchaseEnabled)
					item.Rate = purchaseItem.Rate;
				if (isUpdateItemUOMOnPurchaseEnabled)
					item.UnitOfMeasurement = purchaseItem.UnitOfMeasurement;

				await ItemData.InsertItem(item);
			}
		}
	}
}
