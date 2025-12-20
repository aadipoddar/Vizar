using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.ItemIssue;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Data;

public static class GenerateCodes
{
    public enum CodeType
    {
        Accounting,
        Ledger,
        Purchase,
        PurchaseReturn,
        ItemIssue,
        Item,
        ItemType,
        ItemCategory,
        Manufacturer,
        Service
    }

    private static async Task<string> CheckDuplicateCode(string code, int numberLength, CodeType type)
    {
        var isDuplicate = true;
        while (isDuplicate)
        {
            switch (type)
            {
                case CodeType.Accounting:
                    var accounting = await CommonData.LoadTableDataByTransactionNo<AccountingModel>(TableNames.Accounting, code);
                    isDuplicate = accounting is not null;
                    break;
                case CodeType.Purchase:
                    var purchase = await CommonData.LoadTableDataByTransactionNo<PurchaseModel>(TableNames.Purchase, code);
                    isDuplicate = purchase is not null;
                    break;
                case CodeType.PurchaseReturn:
                    var purchaseReturn = await CommonData.LoadTableDataByTransactionNo<PurchaseReturnModel>(TableNames.PurchaseReturn, code);
                    isDuplicate = purchaseReturn is not null;
                    break;
                case CodeType.ItemIssue:
                    var itemIssue = await CommonData.LoadTableDataByTransactionNo<ItemIssueModel>(TableNames.ItemIssue, code);
                    isDuplicate = itemIssue is not null;
                    break;
                case CodeType.Service:
                    var service = await CommonData.LoadTableDataByTransactionNo<ServiceModel>(TableNames.Service, code);
                    isDuplicate = service is not null;
                    break;

                case CodeType.Ledger:
                    var ledger = await CommonData.LoadTableDataByCode<LedgerModel>(TableNames.Ledger, code);
                    isDuplicate = ledger is not null;
                    break;
                case CodeType.Manufacturer:
                    var manufacturer = await CommonData.LoadTableDataByCode<ManufacturerModel>(TableNames.Manufacturer, code);
                    isDuplicate = manufacturer is not null;
                    break;
                case CodeType.ItemCategory:
                    var itemCategory = await CommonData.LoadTableDataByCode<ItemCategoryModel>(TableNames.ItemCategory, code);
                    isDuplicate = itemCategory is not null;
                    break;
                case CodeType.ItemType:
                    var itemType = await CommonData.LoadTableDataByCode<ItemTypeModel>(TableNames.ItemType, code);
                    isDuplicate = itemType is not null;
                    break;
                case CodeType.Item:
                    var item = await CommonData.LoadTableDataByCode<ItemModel>(TableNames.Item, code);
                    isDuplicate = item is not null;
                    break;
            }

            if (!isDuplicate)
                return code;

            var prefix = code[..(code.Length - numberLength)];
            var lastNumberPart = code[(code.Length - numberLength)..];
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                int nextNumber = lastNumber + 1;
                code = $"{prefix}{nextNumber.ToString($"D{numberLength}")}";
            }
            else
                code = $"{prefix}{1.ToString($"D{numberLength}")}";
        }
        return code;
    }

    public static async Task<string> GenerateAccountingTransactionNo(AccountingModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)).Code;
        var accountingPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix)).Value;

        var lastAccounting = await CommonData.LoadLastTableDataByCompanyFinancialYear<AccountingModel>(TableNames.Accounting, transaction.CompanyId, transaction.FinancialYearId);
        if (lastAccounting is not null)
        {
            var lastTransactionNo = lastAccounting.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + accountingPrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}{nextNumber:D6}", 6, CodeType.Accounting);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}000001", 6, CodeType.Accounting);
    }

    public static async Task<string> GeneratePurchaseTransactionNo(PurchaseModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)).Code;
        var purchasePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseTransactionPrefix)).Value;

        var lastPurchase = await CommonData.LoadLastTableDataByCompanyFinancialYear<PurchaseModel>(TableNames.Purchase, transaction.CompanyId, transaction.FinancialYearId);
        if (lastPurchase is not null)
        {
            var lastTransactionNo = lastPurchase.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{purchasePrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + purchasePrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchasePrefix}{nextNumber:D6}", 6, CodeType.Purchase);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchasePrefix}000001", 6, CodeType.Purchase);
    }

    public static async Task<string> GeneratePurchaseReturnTransactionNo(PurchaseReturnModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)).Code;
        var purchaseReturnPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseReturnTransactionPrefix)).Value;

        var lastPurchase = await CommonData.LoadLastTableDataByCompanyFinancialYear<PurchaseReturnModel>(TableNames.PurchaseReturn, transaction.CompanyId, transaction.FinancialYearId);
        if (lastPurchase is not null)
        {
            var lastTransactionNo = lastPurchase.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{purchaseReturnPrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + purchaseReturnPrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchaseReturnPrefix}{nextNumber:D6}", 6, CodeType.PurchaseReturn);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchaseReturnPrefix}000001", 6, CodeType.PurchaseReturn);
    }

    public static async Task<string> GenerateItemIssueTransactionNo(ItemIssueModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)).Code;
        var itemIssuePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemIssueTransactionPrefix)).Value;

        var lastItemIssue = await CommonData.LoadLastTableDataByFinancialYear<ItemIssueModel>(TableNames.ItemIssue, transaction.FinancialYearId);
        if (lastItemIssue is not null)
        {
            var lastTransactionNo = lastItemIssue.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{itemIssuePrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + itemIssuePrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{itemIssuePrefix}{nextNumber:D6}", 6, CodeType.ItemIssue);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{itemIssuePrefix}000001", 6, CodeType.ItemIssue);
    }

    public static async Task<string> GenerateServiceTransactionNo(ServiceModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId)).Code;
        var servicePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ServiceTransactionPrefix)).Value;

        var lastService = await CommonData.LoadLastTableDataByFinancialYear<ServiceModel>(TableNames.Service, transaction.FinancialYearId);
        if (lastService is not null)
        {
            var lastTransactionNo = lastService.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{servicePrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + servicePrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{servicePrefix}{nextNumber:D6}", 6, CodeType.Service);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{servicePrefix}000001", 6, CodeType.Service);
    }

    public static async Task<string> GenerateItemStockAdjustmentTransactionNo(DateTime transactionDateTime)
    {
        var financialYear = await FinancialYearData.LoadFinancialYearByDateTime(transactionDateTime);
        var settings = await SettingsData.LoadSettingsByKey(SettingsKeys.PrimaryCompanyLinkingId);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, int.Parse(settings.Value))).Code;
        var adjustmentPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemStockAdjustmentTransactionPrefix)).Value;
        var currentDateTime = await CommonData.LoadCurrentDateTime();

        return $"{companyPrefix}{financialYear.YearNo}{adjustmentPrefix}{currentDateTime:ddMMyy}{currentDateTime:HHmmss}";
    }


    public static async Task<string> GenerateLedgerCode()
    {
        var ledgers = await CommonData.LoadTableData<LedgerModel>(TableNames.Ledger);
        var ledgerPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.LedgerCodePrefix)).Value;

        var lastLedger = ledgers.OrderByDescending(l => l.Id).FirstOrDefault();
        if (lastLedger is not null)
        {
            var lastLedgerCode = lastLedger.Code;
            if (lastLedgerCode.StartsWith(ledgerPrefix))
            {
                var lastNumberPart = lastLedgerCode[ledgerPrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{ledgerPrefix}{nextNumber:D6}", 6, CodeType.Ledger);
                }
            }
        }

        return await CheckDuplicateCode($"{ledgerPrefix}000001", 6, CodeType.Ledger);
    }

    public static async Task<string> GenerateItemCode()
    {
        var items = await CommonData.LoadTableData<ItemModel>(TableNames.Item);
        var itemPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemCodePrefix)).Value;

        var lastItem = items.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastItem is not null)
        {
            var lastItemCode = lastItem.Code;
            if (lastItemCode.StartsWith(itemPrefix))
            {
                var lastNumberPart = lastItemCode[itemPrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{itemPrefix}{nextNumber:D6}", 6, CodeType.Item);
                }
            }
        }

        return await CheckDuplicateCode($"{itemPrefix}000001", 6, CodeType.Item);
    }

    public static async Task<string> GenerateItemTypeCode()
    {
        var itemTypes = await CommonData.LoadTableData<ItemTypeModel>(TableNames.ItemType);
        var itemTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemTypeCodePrefix)).Value;

        var lastItemType = itemTypes.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastItemType is not null)
        {
            var lastItemTypeCode = lastItemType.Code;
            if (lastItemTypeCode.StartsWith(itemTypePrefix))
            {
                var lastNumberPart = lastItemTypeCode[itemTypePrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{itemTypePrefix}{nextNumber:D6}", 6, CodeType.ItemType);
                }
            }
        }

        return await CheckDuplicateCode($"{itemTypePrefix}000001", 6, CodeType.ItemType);
    }

    public static async Task<string> GenerateItemCategoryCode()
    {
        var itemCategories = await CommonData.LoadTableData<ItemCategoryModel>(TableNames.ItemCategory);
        var itemCategoryPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemCategoryCodePrefix)).Value;

        var lastItemCategory = itemCategories.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastItemCategory is not null)
        {
            var lastItemCategoryCode = lastItemCategory.Code;
            if (lastItemCategoryCode.StartsWith(itemCategoryPrefix))
            {
                var lastNumberPart = lastItemCategoryCode[itemCategoryPrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{itemCategoryPrefix}{nextNumber:D6}", 6, CodeType.ItemCategory);
                }
            }
        }

        return await CheckDuplicateCode($"{itemCategoryPrefix}000001", 6, CodeType.ItemCategory);
    }

    public static async Task<string> GenerateManufacturerCode()
    {
        var manufacturers = await CommonData.LoadTableData<ManufacturerModel>(TableNames.Manufacturer);
        var manufacturerPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ManufacturerCodePrefix)).Value;

        var lastManufacturer = manufacturers.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastManufacturer is not null)
        {
            var lastManufacturerCode = lastManufacturer.Code;
            if (lastManufacturerCode.StartsWith(manufacturerPrefix))
            {
                var lastNumberPart = lastManufacturerCode[manufacturerPrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{manufacturerPrefix}{nextNumber:D6}", 6, CodeType.Manufacturer);
                }
            }
        }

        return await CheckDuplicateCode($"{manufacturerPrefix}000001", 6, CodeType.Manufacturer);
    }
}
