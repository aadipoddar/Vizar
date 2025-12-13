using VizarLibrary.Data.Accounts.FinancialAccounting;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Inventory.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Data.Inventory.Purchase;

public static class PurchaseReturnData
{
    private static async Task<int> InsertPurchaseReturn(PurchaseReturnModel purchaseReturn) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseReturn, purchaseReturn)).FirstOrDefault();

    private static async Task<int> InsertPurchaseReturnDetail(PurchaseReturnDetailModel purchaseReturnDetail) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertPurchaseReturnDetail, purchaseReturnDetail)).FirstOrDefault();

    public static async Task DeleteTransaction(PurchaseReturnModel purchaseReturn)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, purchaseReturn.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot delete transaction as the financial year is locked.");

        purchaseReturn.Status = false;
        await InsertPurchaseReturn(purchaseReturn);
        await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.PurchaseReturn), purchaseReturn.Id);

        var purchaseReturnVoucher = await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseReturnVoucherId);
        var existingAccounting = await AccountingData.LoadAccountingByVoucherReference(int.Parse(purchaseReturnVoucher.Value), purchaseReturn.Id, purchaseReturn.TransactionNo);
        if (existingAccounting is not null && existingAccounting.Id > 0)
        {
            existingAccounting.Status = false;
            await AccountingData.InsertAccounting(existingAccounting);
        }
    }

    public static async Task RecoverPurchaseReturnTransaction(PurchaseReturnModel purchaseReturn)
    {
        var transactionDetails = await CommonData.LoadTableDataByMasterId<PurchaseReturnDetailModel>(TableNames.PurchaseReturnDetail, purchaseReturn.Id);
        List<PurchaseReturnItemCartModel> purchaseItemCarts = [];

        foreach (var item in transactionDetails)
            purchaseItemCarts.Add(new()
            {
                ItemId = item.ItemId,
                ItemName = "",
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
                InclusiveTax = item.InclusiveTax,
                TotalTaxAmount = item.TotalTaxAmount,
                Total = item.Total,
                NetRate = item.NetRate,
                Remarks = item.Remarks
            });

        await SavePurchaseReturnTransaction(purchaseReturn, purchaseItemCarts);
    }

    public static async Task<int> SavePurchaseReturnTransaction(PurchaseReturnModel purchaseReturn, List<PurchaseReturnItemCartModel> purchaseReturnDetails)
    {
        bool update = purchaseReturn.Id > 0;

        if (update)
        {
            var existingPurchaseReturn = await CommonData.LoadTableDataById<PurchaseReturnModel>(TableNames.PurchaseReturn, purchaseReturn.Id);
            var updateFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingPurchaseReturn.FinancialYearId);
            if (updateFinancialYear is null || updateFinancialYear.Locked || updateFinancialYear.Status == false)
                throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");
        }

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, purchaseReturn.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

        purchaseReturn.Id = await InsertPurchaseReturn(purchaseReturn);
        await SavePurchaseReturnDetail(purchaseReturn, purchaseReturnDetails, update);
        await SaveRawMaterialStock(purchaseReturn, purchaseReturnDetails, update);
        await SaveAccounting(purchaseReturn, update);

        return purchaseReturn.Id;
    }

    private static async Task SavePurchaseReturnDetail(PurchaseReturnModel purchaseReturn, List<PurchaseReturnItemCartModel> purchaseReturnDetails, bool update)
    {
        if (update)
        {
            var existingPurchaseDetails = await CommonData.LoadTableDataByMasterId<PurchaseReturnDetailModel>(TableNames.PurchaseReturnDetail, purchaseReturn.Id);
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
                MasterId = purchaseReturn.Id,
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

    private static async Task SaveRawMaterialStock(PurchaseReturnModel purchaseReturn, List<PurchaseReturnItemCartModel> cart, bool update)
    {
        if (update)
            await ItemStockData.DeleteItemStockByTypeTransactionId(nameof(StockType.PurchaseReturn), purchaseReturn.Id);

        foreach (var item in cart)
            await ItemStockData.InsertItemStock(new()
            {
                Id = 0,
                ItemId = item.ItemId,
                IdentificationNo = item.IdentificationNo,
                Quantity = -item.Quantity,
                NetRate = item.NetRate,
                TransactionId = purchaseReturn.Id,
                Type = nameof(StockType.PurchaseReturn),
                TransactionNo = purchaseReturn.TransactionNo,
                TransactionDateTime = purchaseReturn.TransactionDateTime
            });
    }

    private static async Task SaveAccounting(PurchaseReturnModel purchaseReturn, bool update)
    {
        if (update)
        {
            var purchaseReturnVoucher = await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseReturnVoucherId);
            var existingAccounting = await AccountingData.LoadAccountingByVoucherReference(int.Parse(purchaseReturnVoucher.Value), purchaseReturn.Id, purchaseReturn.TransactionNo);
            if (existingAccounting is not null && existingAccounting.Id > 0)
            {
                existingAccounting.Status = false;
                await AccountingData.InsertAccounting(existingAccounting);
            }
        }

        var purchaseReturnOverview = await CommonData.LoadTableDataById<PurchaseReturnOverviewModel>(ViewNames.PurchaseReturnOverview, purchaseReturn.Id);
        if (purchaseReturnOverview is null)
            return;

        if (purchaseReturnOverview.TotalAmount == 0)
            return;

        var accountingCart = new List<AccountingItemCartModel>();

        if (purchaseReturnOverview.TotalAmount > 0)
            accountingCart.Add(new()
            {
                ReferenceId = purchaseReturnOverview.Id,
                ReferenceType = nameof(ReferenceTypes.PurchaseReturn),
                ReferenceNo = purchaseReturnOverview.TransactionNo,
                LedgerId = purchaseReturnOverview.PartyId,
                Debit = purchaseReturnOverview.TotalAmount,
                Credit = null,
                Remarks = $"Party Account Posting For Purchase Return Bill {purchaseReturnOverview.TransactionNo}",
            });

        if (purchaseReturnOverview.TotalAmount - purchaseReturnOverview.TotalExtraTaxAmount > 0)
        {
            var purchaseLedger = await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseLedgerId);
            accountingCart.Add(new()
            {
                ReferenceId = purchaseReturnOverview.Id,
                ReferenceType = nameof(ReferenceTypes.PurchaseReturn),
                ReferenceNo = purchaseReturnOverview.TransactionNo,
                LedgerId = int.Parse(purchaseLedger.Value),
                Debit = null,
                Credit = purchaseReturnOverview.TotalAmount - purchaseReturnOverview.TotalExtraTaxAmount,
                Remarks = $"Purchase Account Posting For Purchase Return Bill {purchaseReturnOverview.TransactionNo}",
            });
        }

        if (purchaseReturnOverview.TotalExtraTaxAmount > 0)
        {
            var gstLedger = await SettingsData.LoadSettingsByKey(SettingsKeys.GSTLedgerId);
            accountingCart.Add(new()
            {
                ReferenceId = purchaseReturnOverview.Id,
                ReferenceType = nameof(ReferenceTypes.PurchaseReturn),
                ReferenceNo = purchaseReturnOverview.TransactionNo,
                LedgerId = int.Parse(gstLedger.Value),
                Debit = null,
                Credit = purchaseReturnOverview.TotalExtraTaxAmount,
                Remarks = $"GST Account Posting For Purchase Return Bill {purchaseReturnOverview.TransactionNo}",
            });
        }

        var voucher = await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseReturnVoucherId);
        var accounting = new AccountingModel
        {
            Id = 0,
            TransactionNo = "",
            CompanyId = purchaseReturnOverview.CompanyId,
            VoucherId = int.Parse(voucher.Value),
            ReferenceId = purchaseReturnOverview.Id,
            ReferenceNo = purchaseReturnOverview.TransactionNo,
            TransactionDateTime = purchaseReturnOverview.TransactionDateTime,
            FinancialYearId = purchaseReturnOverview.FinancialYearId,
            TotalDebitLedgers = accountingCart.Count(a => a.Debit.HasValue),
            TotalCreditLedgers = accountingCart.Count(a => a.Credit.HasValue),
            TotalDebitAmount = accountingCart.Sum(a => a.Debit ?? 0),
            TotalCreditAmount = accountingCart.Sum(a => a.Credit ?? 0),
            Remarks = purchaseReturnOverview.Remarks,
            CreatedBy = purchaseReturnOverview.CreatedBy,
            CreatedAt = purchaseReturnOverview.CreatedAt,
            CreatedFromPlatform = purchaseReturnOverview.CreatedFromPlatform,
            Status = true
        };

        await AccountingData.SaveAccountingTransaction(accounting, accountingCart);
    }
}