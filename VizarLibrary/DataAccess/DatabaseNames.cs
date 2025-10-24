namespace VizarLibrary.DataAccess;

public static class TableNames
{
	public static string User => "User";
}

public static class StoredProcedureNames
{
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadCurrentDateTime => "Load_CurrentDateTime";
	public static string LoadSettingsByKey => "Load_Settings_By_Key";

	public static string InsertUser => "Insert_User";
	public static string ResetSettings => "Reset_Settings";
	public static string UpdateSettings => "Update_Settings";
}

public static class ViewNames
{
}