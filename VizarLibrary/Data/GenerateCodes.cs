using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts;
using VizarLibrary.Models.Common;
using VizarLibrary.Models.Inventory;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data;

public static class GenerateCodes
{
	public enum CodeType
	{
		Purchase,
		PurchaseReturn,
		Ledger,
		Manufacturer,
		ItemCategory,
		ItemType,
		Item,
	}

	private static async Task<string> CheckDuplicateCode(string code, int numberLength, CodeType type)
	{
		var isDuplicate = true;
		while (isDuplicate)
		{
			switch (type)
			{
				case CodeType.Purchase:
					var purchase = await CommonData.LoadTableDataByTransactionNo<PurchaseModel>(TableNames.Purchase, code);
					isDuplicate = purchase is not null;
					break;
				case CodeType.PurchaseReturn:
					var purchaseReturn = await CommonData.LoadTableDataByTransactionNo<PurchaseReturnModel>(TableNames.PurchaseReturn, code);
					isDuplicate = purchaseReturn is not null;
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

	public static async Task<string> GeneratePurchaseTransactionNo(PurchaseModel purchase)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, purchase.FinancialYearId);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, purchase.CompanyId)).Code;
		var purchasePrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseTransactionPrefix)).Value;

		var lastPurchase = await CommonData.LoadLastTableDataByCompanyFinancialYear<PurchaseModel>(TableNames.Purchase, purchase.CompanyId, purchase.FinancialYearId);
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

	public static async Task<string> GeneratePurchaseReturnTransactionNo(PurchaseReturnModel purchaseReturn)
	{
		var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, purchaseReturn.FinancialYearId);
		var companyPrefix = (await CommonData.LoadTableDataById<CompanyModel>(TableNames.Company, purchaseReturn.CompanyId)).Code;
		var purchaseReturnPrefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.PurchaseReturnTransactionPrefix)).Value;

		var lastPurchase = await CommonData.LoadLastTableDataByCompanyFinancialYear<PurchaseReturnModel>(TableNames.PurchaseReturn, purchaseReturn.CompanyId, purchaseReturn.FinancialYearId);
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

	public static async Task<string> GenerateLedgerCode()
	{
		var ledgers = await CommonData.LoadTableData<LedgerModel>(TableNames.Ledger);
		var prefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.LedgerCodePrefix)).Value;

		if (ledgers.Count == 0)
			return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.Ledger);

		var lastNumberPart = ledgers.LastOrDefault().Code[prefix.Length..];

		if (int.TryParse(lastNumberPart, out int lastNumber))
		{
			int nextNumber = lastNumber + 1;
			return await CheckDuplicateCode($"{prefix}{nextNumber:D6}", 6, CodeType.Ledger);
		}

		return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.Ledger);
	}

	public static async Task<string> GenerateManufactureCode()
	{
		var manufactures = await CommonData.LoadTableData<ManufacturerModel>(TableNames.Manufacturer);
		var prefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ManufacturerCodePrefix)).Value;

		if (manufactures.Count == 0)
			return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.Manufacturer);

		var lastNumberPart = manufactures.LastOrDefault().Code[prefix.Length..];

		if (int.TryParse(lastNumberPart, out int lastNumber))
		{
			int nextNumber = lastNumber + 1;
			return await CheckDuplicateCode($"{prefix}{nextNumber:D6}", 6, CodeType.Manufacturer);
		}

		return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.Manufacturer);
	}

	public static async Task<string> GenerateItemCategoryCode()
	{
		var itemCategories = await CommonData.LoadTableData<ItemCategoryModel>(TableNames.ItemCategory);
		var prefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemCategoryCodePrefix)).Value;

		if (itemCategories.Count == 0)
			return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.ItemCategory);

		var lastNumberPart = itemCategories.LastOrDefault().Code[prefix.Length..];

		if (int.TryParse(lastNumberPart, out int lastNumber))
		{
			int nextNumber = lastNumber + 1;
			return await CheckDuplicateCode($"{prefix}{nextNumber:D6}", 6, CodeType.ItemCategory);
		}

		return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.ItemCategory);
	}

	public static async Task<string> GenerateItemTypeCode()
	{
		var itemTypes = await CommonData.LoadTableData<ItemTypeModel>(TableNames.ItemType);
		var prefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemTypeCodePrefix)).Value;

		if (itemTypes.Count == 0)
			return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.ItemType);

		var lastNumberPart = itemTypes.LastOrDefault().Code[prefix.Length..];

		if (int.TryParse(lastNumberPart, out int lastNumber))
		{
			int nextNumber = lastNumber + 1;
			return await CheckDuplicateCode($"{prefix}{nextNumber:D6}", 6, CodeType.ItemType);
		}

		return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.ItemType);
	}

	public static async Task<string> GenerateItemCode()
	{
		var items = await CommonData.LoadTableData<ItemModel>(TableNames.Item);
		var prefix = (await SettingsData.LoadSettingsByKey(SettingsKeys.ItemCodePrefix)).Value;

		if (items.Count == 0)
			return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.Item);

		var lastNumberPart = items.LastOrDefault().Code[prefix.Length..];

		if (int.TryParse(lastNumberPart, out int lastNumber))
		{
			int nextNumber = lastNumber + 1;
			return await CheckDuplicateCode($"{prefix}{nextNumber:D6}", 6, CodeType.Item);
		}

		return await CheckDuplicateCode($"{prefix}000001", 6, CodeType.Item);
	}
}
