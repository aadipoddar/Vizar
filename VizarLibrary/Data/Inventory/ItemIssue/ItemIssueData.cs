using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Data.Inventory.ItemIssue;

public static class ItemIssueData
{
    public static async Task<int> InsertItemIssue(ItemIssueModel itemIssue) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemIssue, itemIssue)).FirstOrDefault();

    public static async Task<int> InsertItemIssueDetail(ItemIssueDetailModel itemIssueDetail) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertItemIssueDetail, itemIssueDetail)).FirstOrDefault();

    public static async Task<(MemoryStream pdfStream, string fileName)> GenerateAndDownloadInvoice(int transactionId)
    {
   //     try
   //     {
			//// Load saved transaction details
			//var transaction = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, transactionId) ??
   //             throw new InvalidOperationException("Transaction not found.");

   //         // Load transaction details from database
   //         var transactionDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, transaction.Id);
   //         if (transactionDetails is null || transactionDetails.Count == 0)
   //             throw new InvalidOperationException("No transaction details found for the transaction.");

   //         // Load company
   //         var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)
   //             ?? throw new InvalidOperationException("Company or kitchen details not found.");

   //         var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId.Value);

   //         // Convert item issue details to cart items with item names
   //         var items = await CommonData.LoadTableData<ItemModel>(TableNames.Item);
   //         var vehicles = await CommonData.LoadTableData<VehicleModel>(TableNames.Vehicle);
   //         var cartItems = new List<ItemIssueItemCartModel>();
   //         foreach (var detail in transactionDetails)
   //         {
   //             var item = items.FirstOrDefault(i => i.Id == detail.ItemId);
   //             cartItems.Add(new ItemIssueItemCartModel
   //             {
   //                 ItemId = detail.ItemId,
   //                 ItemName = item?.Name ?? "Unknown Item",
   //                 VehicleId = detail.VehicleId,
   //                 VehicleCode = detail.VehicleId is not null ? vehicles.FirstOrDefault(v => v.Id == detail.VehicleId).Code : null,
   //                 VehicleShortCode = detail.VehicleId is not null ? vehicles.FirstOrDefault(v => v.Id == detail.VehicleId).ShortCode : null,
			//		CurrentHour = detail.VehicleId is not null ? detail.CurrentHour : null,
   //                 CurrentKM = detail.VehicleId is not null ? detail.CurrentKM : null,
			//		IdentificationNo = detail.IdentificationNo,
   //                 UnitOfMeasurement = detail.UnitOfMeasurement,
			//		Quantity = detail.Quantity,
   //                 Rate = detail.Rate,
   //                 Total = detail.Total,
   //                 Remarks = detail.Remarks
   //             });
   //         }

   //         // Generate invoice PDF
   //         var pdfStream = await ItemIssueInvoicePDFExport.ExportKitchenIssueInvoiceWithItems(
   //                 transaction,
   //                 cartItems,
   //                 company,
   //                 garage,
   //                 null, // logo path - uses default
   //                 "Item ISSUE INVOICE"
   //             );

   //         // Generate file name
   //         var currentDateTime = await CommonData.LoadCurrentDateTime();
   //         string fileName = $"ITEM_ISSUE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
   //         return (pdfStream, fileName);
   //     }
   //     catch (Exception ex)
   //     {
   //         throw new InvalidOperationException("Failed to generate and download invoice." + ex.Message);
   //     }

        return (null, null);
    }

    public static async Task<(MemoryStream excelStream, string fileName)> GenerateAndDownloadExcelInvoice(int transactionId)
    {
		//try
		//{
		//	// Load saved transaction details
		//	var transaction = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, transactionId) ??
		//		throw new InvalidOperationException("Transaction not found.");

		//	// Load transaction details from database
		//	var transactionDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, transaction.Id);
		//	if (transactionDetails is null || transactionDetails.Count == 0)
		//		throw new InvalidOperationException("No transaction details found for the transaction.");

		//	// Load company
		//	var company = await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)
		//		?? throw new InvalidOperationException("Company or kitchen details not found.");

		//	var garage = await CommonData.LoadTableDataById<GarageModel>(TableNames.Garage, transaction.GarageId.Value);

		//	// Convert item issue details to cart items with item names
		//	var items = await CommonData.LoadTableData<ItemModel>(TableNames.Item);
		//	var vehicles = await CommonData.LoadTableData<VehicleModel>(TableNames.Vehicle);
		//	var cartItems = new List<ItemIssueItemCartModel>();
		//	foreach (var detail in transactionDetails)
		//	{
		//		var item = items.FirstOrDefault(i => i.Id == detail.ItemId);
		//		cartItems.Add(new ItemIssueItemCartModel
		//		{
		//			ItemId = detail.ItemId,
		//			ItemName = item?.Name ?? "Unknown Item",
		//			VehicleId = detail.VehicleId,
		//			VehicleCode = detail.VehicleId is not null ? vehicles.FirstOrDefault(v => v.Id == detail.VehicleId).Code : null,
		//			VehicleShortCode = detail.VehicleId is not null ? vehicles.FirstOrDefault(v => v.Id == detail.VehicleId).ShortCode : null,
		//			CurrentHour = detail.VehicleId is not null ? detail.CurrentHour : null,
		//			CurrentKM = detail.VehicleId is not null ? detail.CurrentKM : null,
		//			IdentificationNo = detail.IdentificationNo,
		//			UnitOfMeasurement = detail.UnitOfMeasurement,
		//			Quantity = detail.Quantity,
		//			Rate = detail.Rate,
		//			Total = detail.Total,
		//			Remarks = detail.Remarks
		//		});
		//	}

		//	// Generate invoice PDF
		//	var pdfStream = await ItemIssueInvoiceExcelExport.ExportKitchenIssueInvoiceWithItems(
		//			transaction,
		//			cartItems,
		//			company,
		//			garage,
		//			null, // logo path - uses default
		//			"Item ISSUE INVOICE"
		//		);

		//	// Generate file name
		//	var currentDateTime = await CommonData.LoadCurrentDateTime();
		//	string fileName = $"ITEM_ISSUE_INVOICE_{transaction.TransactionNo}_{currentDateTime:yyyyMMdd_HHmmss}.pdf";
		//	return (pdfStream, fileName);
		//}
		//catch (Exception ex)
		//{
		//	throw new InvalidOperationException("Failed to generate and download invoice." + ex.Message);
		//}

		return (null, null);
	}

    public static async Task DeleteItemIssue(int transactionId)
    {
        var transaction = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, transactionId);
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot delete transaction as the financial year is locked.");

        if (transaction is not null)
        {
            transaction.Status = false;
            await InsertItemIssue(transaction);
            await ItemStockData.DeleteItemStockByTypeTransactionId(StockType.ItemIssue.ToString(), transaction.Id);
        }
    }

    public static async Task RecoverItemIssueTransaction(ItemIssueModel itemIssue)
    {
        var transactionDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, itemIssue.Id);
        List<ItemIssueItemCartModel> itemIssueItemCarts = [];

        foreach (var item in transactionDetails)
            itemIssueItemCarts.Add(new()
            {
                ItemId = item.ItemId,
                ItemName = "",
                VehicleId = item.VehicleId,
                VehicleCode = "",
                VehicleShortCode = "",
                CurrentHour = item.CurrentHour,
                CurrentKM = item.CurrentKM,
                IdentificationNo = item.IdentificationNo,
                UnitOfMeasurement = item.UnitOfMeasurement,
                Quantity = item.Quantity,
                Rate = item.Rate,
                Total = item.Total,
                Remarks = item.Remarks
            });

        await SaveItemIssueTransaction(itemIssue, itemIssueItemCarts);
    }

    public static async Task<int> SaveItemIssueTransaction(ItemIssueModel itemIssue, List<ItemIssueItemCartModel> itemIssueDetails)
    {
        bool update = itemIssue.Id > 0;

        if (update)
        {
            var existingItemIssue = await CommonData.LoadTableDataById<ItemIssueModel>(TableNames.ItemIssue, itemIssue.Id);
            var updateFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingItemIssue.FinancialYearId);
            if (updateFinancialYear is null || updateFinancialYear.Locked || updateFinancialYear.Status == false)
                throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

            itemIssue.TransactionNo = existingItemIssue.TransactionNo;
        }
        else
            itemIssue.TransactionNo = await GenerateCodes.GenerateItemIssueTransactionNo(itemIssue);

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, itemIssue.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

        itemIssue.Id = await InsertItemIssue(itemIssue);
        await SaveItemIssueDetail(itemIssue, itemIssueDetails, update);
        await SaveItemStock(itemIssue, itemIssueDetails, update);

        return itemIssue.Id;
    }

    private static async Task SaveItemIssueDetail(ItemIssueModel itemIssue, List<ItemIssueItemCartModel> itemIssueDetails, bool update)
    {
        if (update)
        {
            var existingItemIssueDetails = await CommonData.LoadTableDataByMasterId<ItemIssueDetailModel>(TableNames.ItemIssueDetail, itemIssue.Id);
            foreach (var item in existingItemIssueDetails)
            {
                item.Status = false;
                await InsertItemIssueDetail(item);
            }
        }

        foreach (var item in itemIssueDetails)
            await InsertItemIssueDetail(new()
            {
                Id = 0,
                MasterId = itemIssue.Id,
                ItemId = item.ItemId,
                VehicleId = item.VehicleId,
                CurrentHour = item.CurrentHour,
                CurrentKM = item.CurrentKM,
                IdentificationNo = item.IdentificationNo,
				UnitOfMeasurement = item.UnitOfMeasurement,
                Quantity = item.Quantity,
                Rate = item.Rate,
                Total = item.Total,
                Remarks = item.Remarks,
                Status = true
            });
    }

    private static async Task SaveItemStock(ItemIssueModel itemIssue, List<ItemIssueItemCartModel> cart, bool update)
    {
        if (update)
            await ItemStockData.DeleteItemStockByTypeTransactionId(StockType.ItemIssue.ToString(), itemIssue.Id);

        foreach (var item in cart)
            await ItemStockData.InsertItemStock(new()
            {
                Id = 0,
                ItemId = item.ItemId,
                Quantity = -item.Quantity,
                IdentificationNo = item.IdentificationNo,
                NetRate = null,
				Type = StockType.ItemIssue.ToString(),
                TransactionId = itemIssue.Id,
                TransactionNo = itemIssue.TransactionNo,
                TransactionDateTime = itemIssue.TransactionDateTime
            });
    }
}