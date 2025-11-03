using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts;
using VizarLibrary.Models.Inventory;

namespace VizarLibrary.Data.Inventory;

public static class PurchaseReturnData
{
	public static async Task<int> InsertPurchaseReturn(PurchaseReturnModel purchaseReturn) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseReturn, purchaseReturn)).FirstOrDefault();

	public static async Task<int> InsertPurchaseReturnDetail(PurchaseReturnDetailModel purchaseReturnDetail) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseReturnDetail, purchaseReturnDetail)).FirstOrDefault();

	public static async Task<List<PurchaseReturnDetailModel>> LoadPurchaseReturnDetailByPurchaseReturn(int PurchaseReturnId) =>
		await SqlDataAccess.LoadData<PurchaseReturnDetailModel, dynamic>(StoredProcedureNames.LoadPurchaseReturnDetailByPurchaseReturn, new { PurchaseReturnId });

	public static async Task<List<PurchaseReturnOverviewModel>> LoadPurchaseReturnOverviewByDate(DateTime StartDate, DateTime EndDate) =>
		await SqlDataAccess.LoadData<PurchaseReturnOverviewModel, dynamic>(StoredProcedureNames.LoadPurchaseReturnOverviewByDate, new { StartDate, EndDate });

	public static async Task DeletePurchaseReturn(int purchaseReturnId)
	{
		var purchaseReturn = await CommonData.LoadTableDataById<PurchaseReturnModel>(TableNames.PurchaseReturn, purchaseReturnId);
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, purchaseReturn.FinancialYearId);
		if (financialYear is null || financialYear.Locked || financialYear.Status == false)
			throw new InvalidOperationException("Cannot delete purchase return transaction as the financial year is locked.");

		if (purchaseReturn is not null)
		{
			purchaseReturn.Status = false;
			await InsertPurchaseReturn(purchaseReturn);
		}
	}

	public static async Task<int> SavePurchaseReturnTransaction(PurchaseReturnModel purchaseReturn, List<PurchaseReturnItemCartModel> purchaseReturnDetails)
	{
		bool update = purchaseReturn.Id > 0;

		if (update)
		{
			var existingPurchaseReturn = await CommonData.LoadTableDataById<PurchaseReturnModel>(TableNames.PurchaseReturn, purchaseReturn.Id);
			var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingPurchaseReturn.FinancialYearId);
			if (financialYear is null || financialYear.Locked || financialYear.Status == false)
				throw new InvalidOperationException("Cannot update purchase return transaction as the financial year is locked.");
		}

		purchaseReturn.Id = await InsertPurchaseReturn(purchaseReturn);
		await SavePurchaseReturnDetail(purchaseReturn, purchaseReturnDetails, update);

		return purchaseReturn.Id;
	}

	private static async Task SavePurchaseReturnDetail(PurchaseReturnModel purchaseReturn, List<PurchaseReturnItemCartModel> purchaseReturnDetails, bool update)
	{
		if (update)
		{
			var existingPurchaseDetails = await LoadPurchaseReturnDetailByPurchaseReturn(purchaseReturn.Id);
			foreach (var item in existingPurchaseDetails)
			{
				item.Status = false;
				await InsertPurchaseReturnDetail(item);
			}
		}

		foreach (var item in purchaseReturnDetails)
			await InsertPurchaseReturnDetail(new()
			{
				Id = 0,
				PurchaseReturnId = purchaseReturn.Id,
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
}