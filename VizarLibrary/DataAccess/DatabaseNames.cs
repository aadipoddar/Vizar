namespace VizarLibrary.DataAccess;

public static class TableNames
{
	public static string Settings => "Settings";
	public static string User => "User";
	public static string Company => "Company";
	public static string Ledger => "Ledger";
	public static string Tax => "Tax";
	public static string State => "State";
	public static string FinancialYear => "FinancialYear";
	public static string Purchase => "Purchase";
	public static string PurchaseDetail => "PurchaseDetail";
	public static string PurchaseReturn => "PurchaseReturn";
	public static string PurchaseReturnDetail => "PurchaseReturnDetail";
	public static string Item => "Item";
	public static string ItemCategory => "ItemCategory";
	public static string ItemType => "ItemType";
	public static string Manufacturer => "Manufacturer";
}

public static class StoredProcedureNames
{
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadTableDataByCode => "Load_TableData_By_Code";
	public static string LoadTableDataByTransactionNo => "Load_TableData_By_TransactionNo";
	public static string LoadLastTableDataByCompanyFinancialYear => "Load_LastTableData_By_Company_FinancialYear";
	public static string LoadCurrentDateTime => "Load_CurrentDateTime";
	public static string LoadSettingsByKey => "Load_Settings_By_Key";

	public static string LoadFinancialYearByDateTime => "Load_FinancialYear_By_DateTime";

	public static string LoadItemByPartyPurchaseDateTime => "Load_Item_By_Party_PurchaseDateTime";

	public static string LoadPurchaseDetailByPurchase => "Load_PurchaseDetail_By_Purchase";
	public static string LoadPurchaseOverviewByDate => "Load_PurchaseOverview_By_Date";
	public static string LoadPurchaseReturnDetailByPurchaseReturn => "Load_PurchaseReturnDetail_By_PurchaseReturn";
	public static string LoadPurchaseReturnOverviewByDate => "Load_PurchaseReturnOverview_By_Date";

	public static string InsertUser => "Insert_User";
	public static string ResetSettings => "Reset_Settings";
	public static string UpdateSettings => "Update_Settings";

	public static string InsertManufacturer => "Insert_Manufacturer";
	public static string InsertItemCategory => "Insert_ItemCategory";
	public static string InsertItemType => "Insert_ItemType";
	public static string InsertItem => "Insert_Item";

	public static string InsertPurchase => "Insert_Purchase";
	public static string InsertPurchaseDetail => "Insert_PurchaseDetail";
	public static string InsertPurchaseReturn => "Insert_PurchaseReturn";
	public static string InsertPurchaseReturnDetail => "Insert_PurchaseReturnDetail";

	public static string InsertLedger => "Insert_Ledger";
}

public static class ViewNames
{
	public static string PurchaseOverview => "Purchase_Overview";
	public static string PurchaseReturnOverview => "PurchaseReturn_Overview";
	public static string ItemsPurchaseOverview => "Items_Purchase_Overview";
	public static string ItemsPurchaseReturnOverview => "Items_PurchaseReturn_Overview";
}