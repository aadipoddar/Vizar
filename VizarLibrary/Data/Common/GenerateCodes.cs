using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Operations;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Document;
using VizarLibrary.Models.Fleet.Repair;
using VizarLibrary.Models.Fleet.Service;
using VizarLibrary.Models.Fleet.Vehicle;
using VizarLibrary.Models.Inventory.Item;
using VizarLibrary.Models.Inventory.Purchase;
using VizarLibrary.Models.Operations;

namespace VizarLibrary.Data.Common;

public static class GenerateCodes
{
    public enum CodeType
    {
        Accounting,
        Ledger,
        Purchase,
        PurchaseReturn,
        InsideRepair,
        OutsideRepair,
        Service,
        Item,
        ItemType,
        ItemCategory,
        DocumentType,
        ServiceType,
        Manufacturer,
        VehicleType
    }

    private static async Task<string> CheckDuplicateCode(string code, int numberLength, CodeType type, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var isDuplicate = true;
        while (isDuplicate)
        {
            switch (type)
            {
                case CodeType.Accounting:
                    var accounting = await CommonData.LoadTableDataByTransactionNo<AccountingModel>(TableNames.Accounting, code, sqlDataAccessTransaction);
                    isDuplicate = accounting is not null;
                    break;
                case CodeType.Purchase:
                    var purchase = await CommonData.LoadTableDataByTransactionNo<PurchaseModel>(TableNames.Purchase, code, sqlDataAccessTransaction);
                    isDuplicate = purchase is not null;
                    break;
                case CodeType.PurchaseReturn:
                    var purchaseReturn = await CommonData.LoadTableDataByTransactionNo<PurchaseReturnModel>(TableNames.PurchaseReturn, code, sqlDataAccessTransaction);
                    isDuplicate = purchaseReturn is not null;
                    break;
                case CodeType.InsideRepair:
                    var insideRepair = await CommonData.LoadTableDataByTransactionNo<InsideRepairModel>(TableNames.InsideRepair, code, sqlDataAccessTransaction);
                    isDuplicate = insideRepair is not null;
                    break;
                case CodeType.OutsideRepair:
                    var outsideRepair = await CommonData.LoadTableDataByTransactionNo<OutsideRepairModel>(TableNames.OutsideRepair, code, sqlDataAccessTransaction);
                    isDuplicate = outsideRepair is not null;
                    break;
                case CodeType.Service:
                    var service = await CommonData.LoadTableDataByTransactionNo<ServiceModel>(TableNames.Service, code, sqlDataAccessTransaction);
                    isDuplicate = service is not null;
                    break;

                case CodeType.Ledger:
                    var ledger = await CommonData.LoadTableDataByCode<LedgerModel>(TableNames.Ledger, code, sqlDataAccessTransaction);
                    isDuplicate = ledger is not null;
                    break;
                case CodeType.Manufacturer:
                    var manufacturer = await CommonData.LoadTableDataByCode<ManufacturerModel>(TableNames.Manufacturer, code, sqlDataAccessTransaction);
                    isDuplicate = manufacturer is not null;
                    break;
                case CodeType.ItemCategory:
                    var itemCategory = await CommonData.LoadTableDataByCode<ItemCategoryModel>(TableNames.ItemCategory, code, sqlDataAccessTransaction);
                    isDuplicate = itemCategory is not null;
                    break;
                case CodeType.DocumentType:
                    var documentType = await CommonData.LoadTableDataByCode<DocumentTypeModel>(TableNames.DocumentType, code, sqlDataAccessTransaction);
                    isDuplicate = documentType is not null;
                    break;
                case CodeType.ServiceType:
                    var serviceType = await CommonData.LoadTableDataByCode<ServiceTypeModel>(TableNames.ServiceType, code, sqlDataAccessTransaction);
                    isDuplicate = serviceType is not null;
                    break;
                case CodeType.ItemType:
                    var itemType = await CommonData.LoadTableDataByCode<ItemTypeModel>(TableNames.ItemType, code, sqlDataAccessTransaction);
                    isDuplicate = itemType is not null;
                    break;
                case CodeType.Item:
                    var item = await CommonData.LoadTableDataByCode<ItemModel>(TableNames.Item, code, sqlDataAccessTransaction);
                    isDuplicate = item is not null;
                    break;
                case CodeType.VehicleType:
                    var vehicleType = await CommonData.LoadTableDataByCode<VehicleTypeModel>(TableNames.VehicleType, code, sqlDataAccessTransaction);
                    isDuplicate = vehicleType is not null;
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

    public static async Task<string> GenerateAccountingTransactionNo(AccountingModel transaction, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId, sqlDataAccessTransaction);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId, sqlDataAccessTransaction)).Code;
        var accountingPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.FinancialAccountingTransactionPrefix, sqlDataAccessTransaction)).Value;

        var lastAccounting = await CommonData.LoadLastTableDataByCompanyFinancialYear<AccountingModel>(TableNames.Accounting, transaction.CompanyId, transaction.FinancialYearId, sqlDataAccessTransaction);
        if (lastAccounting is not null)
        {
            var lastTransactionNo = lastAccounting.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + accountingPrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}{nextNumber:D6}", 6, CodeType.Accounting, sqlDataAccessTransaction);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{accountingPrefix}000001", 6, CodeType.Accounting, sqlDataAccessTransaction);
    }

    public static async Task<string> GeneratePurchaseTransactionNo(PurchaseModel transaction, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId, sqlDataAccessTransaction);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId, sqlDataAccessTransaction)).Code;
        var purchasePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseTransactionPrefix, sqlDataAccessTransaction)).Value;

        var lastPurchase = await CommonData.LoadLastTableDataByCompanyFinancialYear<PurchaseModel>(TableNames.Purchase, transaction.CompanyId, transaction.FinancialYearId, sqlDataAccessTransaction);
        if (lastPurchase is not null)
        {
            var lastTransactionNo = lastPurchase.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{purchasePrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + purchasePrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchasePrefix}{nextNumber:D6}", 6, CodeType.Purchase, sqlDataAccessTransaction);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchasePrefix}000001", 6, CodeType.Purchase, sqlDataAccessTransaction);
    }

    public static async Task<string> GeneratePurchaseReturnTransactionNo(PurchaseReturnModel transaction, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId, sqlDataAccessTransaction);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId, sqlDataAccessTransaction)).Code;
        var purchaseReturnPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseReturnTransactionPrefix, sqlDataAccessTransaction)).Value;

        var lastPurchase = await CommonData.LoadLastTableDataByCompanyFinancialYear<PurchaseReturnModel>(TableNames.PurchaseReturn, transaction.CompanyId, transaction.FinancialYearId, sqlDataAccessTransaction);
        if (lastPurchase is not null)
        {
            var lastTransactionNo = lastPurchase.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{purchaseReturnPrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + purchaseReturnPrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchaseReturnPrefix}{nextNumber:D6}", 6, CodeType.PurchaseReturn, sqlDataAccessTransaction);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{purchaseReturnPrefix}000001", 6, CodeType.PurchaseReturn, sqlDataAccessTransaction);
    }

    public static async Task<string> GenerateInsideRepairTransactionNo(InsideRepairModel transaction, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId, sqlDataAccessTransaction);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId, sqlDataAccessTransaction)).Code;
        var insideRepairPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.InsideRepairTransactionPrefix, sqlDataAccessTransaction)).Value;

        var lastInsideRepair = await CommonData.LoadLastTableDataByFinancialYear<InsideRepairModel>(TableNames.InsideRepair, transaction.FinancialYearId, sqlDataAccessTransaction);
        if (lastInsideRepair is not null)
        {
            var lastTransactionNo = lastInsideRepair.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{insideRepairPrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + insideRepairPrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{insideRepairPrefix}{nextNumber:D6}", 6, CodeType.InsideRepair, sqlDataAccessTransaction);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{insideRepairPrefix}000001", 6, CodeType.InsideRepair, sqlDataAccessTransaction);
    }

    public static async Task<string> GenerateOutsideRepairTransactionNo(OutsideRepairModel transaction, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId, sqlDataAccessTransaction);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId, sqlDataAccessTransaction)).Code;
        var outsideRepairPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.OutsideRepairTransactionPrefix, sqlDataAccessTransaction)).Value;

        var lastOutsideRepair = await CommonData.LoadLastTableDataByFinancialYear<OutsideRepairModel>(TableNames.OutsideRepair, transaction.FinancialYearId, sqlDataAccessTransaction);
        if (lastOutsideRepair is not null)
        {
            var lastTransactionNo = lastOutsideRepair.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{outsideRepairPrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + outsideRepairPrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{outsideRepairPrefix}{nextNumber:D6}", 6, CodeType.InsideRepair, sqlDataAccessTransaction);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{outsideRepairPrefix}000001", 6, CodeType.OutsideRepair, sqlDataAccessTransaction);
    }

    public static async Task<string> GenerateServiceTransactionNo(ServiceModel transaction, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId, sqlDataAccessTransaction);
        var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, transaction.CompanyId, sqlDataAccessTransaction)).Code;
        var servicePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ServiceTransactionPrefix, sqlDataAccessTransaction)).Value;

        var lastService = await CommonData.LoadLastTableDataByFinancialYear<ServiceModel>(TableNames.Service, transaction.FinancialYearId, sqlDataAccessTransaction);
        if (lastService is not null)
        {
            var lastTransactionNo = lastService.TransactionNo;
            if (lastTransactionNo.StartsWith($"{companyPrefix}{financialYear.YearNo}{servicePrefix}"))
            {
                var lastNumberPart = lastTransactionNo[(companyPrefix.Length + financialYear.YearNo.ToString().Length + servicePrefix.Length)..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{servicePrefix}{nextNumber:D6}", 6, CodeType.Service, sqlDataAccessTransaction);
                }
            }
        }

        return await CheckDuplicateCode($"{companyPrefix}{financialYear.YearNo}{servicePrefix}000001", 6, CodeType.Service, sqlDataAccessTransaction);
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

    public static async Task<string> GenerateDocumentTypeCode()
    {
        var documentTypes = await CommonData.LoadTableData<DocumentTypeModel>(TableNames.DocumentType);
        var documentTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.DocumentTypeCodePrefix)).Value;

        var lastDocumentType = documentTypes.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastDocumentType is not null)
        {
            var lastDocumentTypeCode = lastDocumentType.Code;
            if (lastDocumentTypeCode.StartsWith(documentTypePrefix))
            {
                var lastNumberPart = lastDocumentTypeCode[documentTypePrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{documentTypePrefix}{nextNumber:D6}", 6, CodeType.DocumentType);
                }
            }
        }

        return await CheckDuplicateCode($"{documentTypePrefix}000001", 6, CodeType.DocumentType);
    }

    public static async Task<string> GenerateServiceTypeCode()
    {
        var serviceTypes = await CommonData.LoadTableData<ServiceTypeModel>(TableNames.ServiceType);
        var serviceTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ServiceTypeCodePrefix)).Value;

        var lastServiceType = serviceTypes.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastServiceType is not null)
        {
            var lastServiceTypeCode = lastServiceType.Code;
            if (lastServiceTypeCode.StartsWith(serviceTypePrefix))
            {
                var lastNumberPart = lastServiceTypeCode[serviceTypePrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{serviceTypePrefix}{nextNumber:D6}", 6, CodeType.ServiceType);
                }
            }
        }

        return await CheckDuplicateCode($"{serviceTypePrefix}000001", 6, CodeType.ServiceType);
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

    public static async Task<string> GenerateVehicleTypeCode()
    {
        var vehicleTypes = await CommonData.LoadTableData<VehicleTypeModel>(TableNames.VehicleType);
        var vehicleTypePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.VehicleTypeCodePrefix)).Value;

        var lastVehicleType = vehicleTypes.OrderByDescending(r => r.Id).FirstOrDefault();
        if (lastVehicleType is not null)
        {
            var lastVehicleTypeCode = lastVehicleType.Code;
            if (lastVehicleTypeCode.StartsWith(vehicleTypePrefix))
            {
                var lastNumberPart = lastVehicleTypeCode[vehicleTypePrefix.Length..];
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    int nextNumber = lastNumber + 1;
                    return await CheckDuplicateCode($"{vehicleTypePrefix}{nextNumber:D6}", 6, CodeType.VehicleType);
                }
            }
        }

        return await CheckDuplicateCode($"{vehicleTypePrefix}000001", 6, CodeType.VehicleType);
    }
}
