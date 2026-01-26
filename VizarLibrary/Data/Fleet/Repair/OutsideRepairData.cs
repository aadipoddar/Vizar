using VizarLibrary.Data.Accounts.FinancialAccounting;
using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Operations;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Repair;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Operations;

namespace VizarLibrary.Data.Fleet.Repair;

public static class OutsideRepairData
{
    private static async Task<int> InsertOutsideRepair(OutsideRepairModel outsideRepair, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertOutsideRepair, outsideRepair, sqlDataAccessTransaction)).FirstOrDefault();

    private static async Task<int> InsertOutsideRepairDetail(OutsideRepairDetailModel outsideRepairDetail, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertOutsideRepairDetail, outsideRepairDetail, sqlDataAccessTransaction)).FirstOrDefault();

    public static List<OutsideRepairDetailModel> ConvertCartToDetails(List<OutsideRepairItemCartModel> cart, int insideRepairId) =>
        [.. cart.Select(item => new OutsideRepairDetailModel
        {
            Id = 0,
            MasterId = insideRepairId,
            Job = item.Job,
            Quantity = item.Quantity,
            Rate = item.Rate,
            Total = item.Total,
            Remarks = item.Remarks,
            Status = true
        })];

    public static async Task DeleteTransaction(OutsideRepairModel outsideRepair)
    {
        using SqlDataAccessTransaction sqlDataAccessTransaction = new();

        try
        {
            sqlDataAccessTransaction.StartTransaction();

            await FinancialYearData.ValidateFinancialYear(outsideRepair.TransactionDateTime, sqlDataAccessTransaction);

            outsideRepair.Status = false;
            await InsertOutsideRepair(outsideRepair, sqlDataAccessTransaction);
            sqlDataAccessTransaction.CommitTransaction();

            await OutsideRepairNotify.Notify(outsideRepair.Id, NotifyType.Deleted);
        }
        catch
        {
            sqlDataAccessTransaction.RollbackTransaction();
            throw;
        }
    }

    public static async Task RecoverTransaction(OutsideRepairModel outsideRepair)
    {
        outsideRepair.Status = true;
        var outsideRepairDetails = await CommonData.LoadTableDataByMasterId<OutsideRepairDetailModel>(TableNames.OutsideRepairDetail, outsideRepair.Id);

        await SaveTransaction(outsideRepair, null, outsideRepairDetails);

        await OutsideRepairNotify.Notify(outsideRepair.Id, NotifyType.Recovered);
    }

    public static async Task<int> SaveTransaction(OutsideRepairModel outsideRepair, List<OutsideRepairItemCartModel> cart, List<OutsideRepairDetailModel> outsideRepairDetails = null, bool showNotification = true, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        bool update = outsideRepair.Id > 0;

        if (sqlDataAccessTransaction is null)
        {
            (MemoryStream, string)? previousInvoice = null;
            if (update)
                previousInvoice = await OutsideRepairInvoiceExport.ExportInvoice(outsideRepair.Id, InvoiceExportType.PDF);

            using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

            try
            {
                newSqlDataAccessTransaction.StartTransaction();
                outsideRepair.Id = await SaveTransaction(outsideRepair, cart, outsideRepairDetails, showNotification, newSqlDataAccessTransaction);
                newSqlDataAccessTransaction.CommitTransaction();
            }
            catch
            {
                newSqlDataAccessTransaction.RollbackTransaction();
                throw;
            }

            if (showNotification)
                await OutsideRepairNotify.Notify(outsideRepair.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

            return outsideRepair.Id;
        }

        if (update)
        {
            var existingOutsideRepair = await CommonData.LoadTableDataById<OutsideRepairModel>(TableNames.OutsideRepair, outsideRepair.Id, sqlDataAccessTransaction);
            await FinancialYearData.ValidateFinancialYear(existingOutsideRepair.TransactionDateTime, sqlDataAccessTransaction);
        }

        await FinancialYearData.ValidateFinancialYear(outsideRepair.TransactionDateTime, sqlDataAccessTransaction);

        outsideRepair.Id = await InsertOutsideRepair(outsideRepair, sqlDataAccessTransaction);
        outsideRepairDetails ??= ConvertCartToDetails(cart, outsideRepair.Id);
        await SaveTransactionDetail(outsideRepair, outsideRepairDetails, update, sqlDataAccessTransaction);
        await SaveAccounting(outsideRepair, update, sqlDataAccessTransaction);

        return outsideRepair.Id;
    }

    private static async Task SaveTransactionDetail(OutsideRepairModel outsideRepair, List<OutsideRepairDetailModel> outsideRepairDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (outsideRepairDetails is null || outsideRepairDetails.Count != outsideRepair.TotalItems || outsideRepairDetails.Sum(d => d.Quantity) != outsideRepair.TotalQuantity)
            throw new InvalidOperationException("Item issue details do not match the transaction summary.");

        if (outsideRepairDetails.Any(d => !d.Status))
            throw new InvalidOperationException("Item issue detail items must be active.");

        if (update)
        {
            var existingOutsideRepairDetails = await CommonData.LoadTableDataByMasterId<OutsideRepairDetailModel>(TableNames.OutsideRepairDetail, outsideRepair.Id, sqlDataAccessTransaction);
            foreach (var item in existingOutsideRepairDetails)
            {
                item.Status = false;
                await InsertOutsideRepairDetail(item, sqlDataAccessTransaction);
            }
        }

        foreach (var item in outsideRepairDetails)
        {
            item.MasterId = outsideRepair.Id;
            var id = await InsertOutsideRepairDetail(item, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save item issue detail item.");
        }
    }

    private static async Task SaveAccounting(OutsideRepairModel outsideRepair, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (update)
        {
            var outsideRepairVoucher = await SettingsData.LoadSettingsByKey(SettingsKeys.OutsideRepairVoucherId, sqlDataAccessTransaction);
            var existingAccounting = await AccountingData.LoadAccountingByVoucherReference(int.Parse(outsideRepairVoucher.Value), outsideRepair.Id, outsideRepair.TransactionNo, sqlDataAccessTransaction);
            if (existingAccounting is not null && existingAccounting.Id > 0)
            {
                existingAccounting.Status = false;
                existingAccounting.LastModifiedBy = outsideRepair.LastModifiedBy;
                existingAccounting.LastModifiedAt = outsideRepair.LastModifiedAt;
                existingAccounting.LastModifiedFromPlatform = outsideRepair.LastModifiedFromPlatform;

                await AccountingData.DeleteTransaction(existingAccounting, sqlDataAccessTransaction);
            }
        }

        var outsideRepairOverview = await CommonData.LoadTableDataById<OutsideRepairOverviewModel>(ViewNames.OutsideRepairOverview, outsideRepair.Id, sqlDataAccessTransaction);
        if (outsideRepairOverview is null)
            return;

        if (outsideRepairOverview.TotalAmount == 0)
            return;

        var outsideRepairLedger = await SettingsData.LoadSettingsByKey(SettingsKeys.OutsideRepairLedgerId, sqlDataAccessTransaction);

        var accountingCart = new List<AccountingItemCartModel>
        {
            new()
            {
                ReferenceId = outsideRepairOverview.Id,
                ReferenceType = nameof(ReferenceTypes.OutsideRepair),
                ReferenceNo = outsideRepairOverview.TransactionNo,
                LedgerId = outsideRepairOverview.VendorId,
                Debit = null,
                Credit = outsideRepairOverview.TotalAmount,
                Remarks = $"Vendor Account Posting For Outside Repair Bill {outsideRepairOverview.TransactionNo}",
            },

            new()
            {
                ReferenceId = outsideRepairOverview.Id,
                ReferenceType = nameof(ReferenceTypes.OutsideRepair),
                ReferenceNo = outsideRepairOverview.TransactionNo,
                LedgerId = int.Parse(outsideRepairLedger.Value),
                Debit = outsideRepairOverview.TotalAmount,
                Credit = null,
                Remarks = $"Outside Repair Account Posting For Outside Repair Bill {outsideRepairOverview.TransactionNo}",
            }
        };

        var voucher = await SettingsData.LoadSettingsByKey(SettingsKeys.OutsideRepairVoucherId, sqlDataAccessTransaction);
        var accounting = new AccountingModel
        {
            Id = 0,
            TransactionNo = "",
            CompanyId = outsideRepairOverview.CompanyId,
            VoucherId = int.Parse(voucher.Value),
            ReferenceId = outsideRepairOverview.Id,
            ReferenceNo = outsideRepairOverview.TransactionNo,
            TransactionDateTime = outsideRepairOverview.TransactionDateTime,
            FinancialYearId = outsideRepairOverview.FinancialYearId,
            TotalDebitLedgers = accountingCart.Count(a => a.Debit.HasValue),
            TotalCreditLedgers = accountingCart.Count(a => a.Credit.HasValue),
            TotalDebitAmount = accountingCart.Sum(a => a.Debit ?? 0),
            TotalCreditAmount = accountingCart.Sum(a => a.Credit ?? 0),
            Remarks = outsideRepairOverview.Remarks,
            CreatedBy = outsideRepairOverview.CreatedBy,
            CreatedAt = outsideRepairOverview.CreatedAt,
            CreatedFromPlatform = outsideRepairOverview.CreatedFromPlatform,
            Status = true
        };

        await AccountingData.SaveTransaction(accounting, accountingCart, null, false, sqlDataAccessTransaction);
    }
}
