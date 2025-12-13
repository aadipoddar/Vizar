using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Data.Accounts.FinancialAccounting;

public static class AccountingData
{
    public static async Task<int> InsertAccounting(AccountingModel accounting) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertAccounting, accounting)).FirstOrDefault();

    private static async Task<int> InsertAccountingDetail(AccountingDetailModel accountingDetails) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertAccountingDetail, accountingDetails)).FirstOrDefault();

    public static async Task<AccountingModel> LoadAccountingByVoucherReference(int VoucherId, int ReferenceId, string ReferenceNo) =>
        (await SqlDataAccess.LoadData<AccountingModel, dynamic>(StoredProcedureNames.LoadAccountingByVoucherReference, new { VoucherId, ReferenceId, ReferenceNo })).FirstOrDefault();

    public static async Task<List<TrialBalanceModel>> LoadTrialBalanceByDate(DateTime StartDate, DateTime EndDate) =>
        await SqlDataAccess.LoadData<TrialBalanceModel, dynamic>(StoredProcedureNames.LoadTrialBalanceByDate, new { StartDate, EndDate });

    public static async Task DeleteTransaction(AccountingModel accounting)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, accounting.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot delete transaction as the financial year is locked.");

        accounting.Status = false;
        await InsertAccounting(accounting);
    }

    public static async Task RecoverTransaction(AccountingModel accounting)
    {
        var accountingDetails = await CommonData.LoadTableDataByMasterId<AccountingDetailModel>(TableNames.AccountingDetail, accounting.Id);
        List<AccountingItemCartModel> accountingItemCarts = [];

        foreach (var item in accountingDetails)
            accountingItemCarts.Add(new()
            {
                LedgerName = string.Empty,
                LedgerId = item.LedgerId,
                ReferenceType = item.ReferenceType,
                Credit = item.Credit,
                Debit = item.Debit,
                ReferenceId = item.ReferenceId,
                ReferenceNo = item.ReferenceId.HasValue ? item.ReferenceId.Value.ToString() : null,
                Remarks = item.Remarks
            });

        await SaveAccountingTransaction(accounting, accountingItemCarts);
    }

    public static async Task<int> SaveAccountingTransaction(AccountingModel accounting, List<AccountingItemCartModel> accountingDetails)
    {
        bool update = accounting.Id > 0;

        if (update)
        {
            var existingAccounting = await CommonData.LoadTableDataById<AccountingModel>(TableNames.Accounting, accounting.Id);
            var updateFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingAccounting.FinancialYearId);
            if (updateFinancialYear is null || updateFinancialYear.Locked || updateFinancialYear.Status == false)
                throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

            accounting.TransactionNo = existingAccounting.TransactionNo;
        }
        else
            accounting.TransactionNo = await GenerateCodes.GenerateAccountingTransactionNo(accounting);

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, accounting.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

        accounting.Id = await InsertAccounting(accounting);
        await SaveAccountingDetail(accounting, accountingDetails, update);

        return accounting.Id;
    }

    private static async Task SaveAccountingDetail(AccountingModel accounting, List<AccountingItemCartModel> accountingDetails, bool update)
    {
        if (update)
        {
            var existingAccountingDetails = await CommonData.LoadTableDataByMasterId<AccountingDetailModel>(TableNames.AccountingDetail, accounting.Id);
            foreach (var item in existingAccountingDetails)
            {
                item.Status = false;
                await InsertAccountingDetail(item);
            }
        }

        foreach (var item in accountingDetails)
            await InsertAccountingDetail(new()
            {
                Id = 0,
                MasterId = accounting.Id,
                LedgerId = item.LedgerId,
                Credit = item.Credit,
                Debit = item.Debit,
                ReferenceType = item.ReferenceType,
                ReferenceId = item.ReferenceId,
                ReferenceNo = item.ReferenceNo,
                Remarks = item.Remarks,
                Status = true
            });
    }
}
