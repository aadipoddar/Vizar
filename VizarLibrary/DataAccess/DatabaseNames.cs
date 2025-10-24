namespace VizarLibrary.DataAccess;

public static class TableNames
{
	public static string Settings => "Settings";
	public static string User => "User";
	public static string Ledger => "Ledger";
	public static string Tax => "Tax";
	public static string State => "State";
	public static string FinancialYear => "FinancialYear";
	public static string Purchase => "Purchase";
	public static string PurchaseDetail => "PurchaseDetail";
	public static string Item => "Item";
}

public static class StoredProcedureNames
{
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadCurrentDateTime => "Load_CurrentDateTime";
	public static string LoadSettingsByKey => "Load_Settings_By_Key";

	public static string LoadFinancialYearByDateTime => "Load_FinancialYear_By_DateTime";

	public static string LoadItemByPartyPurchaseDateTime => "Load_Item_By_Party_PurchaseDateTime";

	public static string InsertUser => "Insert_User";
	public static string ResetSettings => "Reset_Settings";
	public static string UpdateSettings => "Update_Settings";
}

public static class ViewNames
{
}