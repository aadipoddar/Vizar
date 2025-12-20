namespace VizarLibrary.DataAccess;

public static class TableNames
{
    public static string Settings => "Settings";
    public static string User => "User";

    public static string Company => "Company";
    public static string Ledger => "Ledger";
    public static string Group => "Group";
    public static string AccountType => "AccountType";
    public static string Voucher => "Voucher";
    public static string StateUT => "StateUT";
    public static string FinancialYear => "FinancialYear";

    public static string Accounting => "Accounting";
    public static string AccountingDetail => "AccountingDetail";

    public static string Item => "Item";
    public static string ItemCategory => "ItemCategory";
    public static string ItemType => "ItemType";
    public static string Manufacturer => "Manufacturer";
    public static string Tax => "Tax";
    public static string ItemStock => "ItemStock";

    public static string Purchase => "Purchase";
    public static string PurchaseDetail => "PurchaseDetail";
    public static string PurchaseReturn => "PurchaseReturn";
    public static string PurchaseReturnDetail => "PurchaseReturnDetail";

    public static string ItemIssue => "ItemIssue";
    public static string ItemIssueDetail => "ItemIssueDetail";

    public static string Vehicle => "Vehicle";
    public static string VehicleModel => "VehicleModel";
    public static string VehicleType => "VehicleType";

    public static string Garage => "Garage";
    public static string ServiceSchedule => "ServiceSchedule";
    public static string ServiceType => "ServiceType";

    public static string Service => "Service";
    public static string ServiceDetail => "ServiceDetail";
}

public static class StoredProcedureNames
{
    public static string LoadTableData => "Load_TableData";
    public static string LoadTableDataById => "Load_TableData_By_Id";
    public static string LoadTableDataByStatus => "Load_TableData_By_Status";
    public static string LoadTableDataByMasterId => "Load_TableData_By_MasterId";
    public static string LoadTableDataByCode => "Load_TableData_By_Code";
    public static string LoadTableDataByTransactionNo => "Load_TableData_By_TransactionNo";
    public static string LoadTableDataByDate => "Load_TableData_By_Date";
    public static string LoadLastTableDataByFinancialYear => "Load_LastTableData_By_FinancialYear";
    public static string LoadLastTableDataByCompanyFinancialYear => "Load_LastTableData_By_Company_FinancialYear";
    public static string LoadCurrentDateTime => "Load_CurrentDateTime";
    public static string LoadSettingsByKey => "Load_Settings_By_Key";

    public static string LoadFinancialYearByDateTime => "Load_FinancialYear_By_DateTime";
    public static string LoadAccountingByVoucherReference => "Load_Accounting_By_Voucher_Reference";
    public static string LoadTrialBalanceByDate => "Load_TrialBalance_By_Date";

    public static string LoadItemByPartyPurchaseDateTime => "Load_Item_By_Party_PurchaseDateTime";
    public static string LoadItemStockSummaryByDate => "Load_ItemStockSummary_By_Date";

    public static string LoadLastVehicleServiceItemByVehicleServiceTypeDate => "Load_Last_VehicleService_Item_By_Vehicle_ServiceType_Date";

    public static string InsertUser => "Insert_User";
    public static string ResetSettings => "Reset_Settings";
    public static string UpdateSettings => "Update_Settings";

    public static string InsertStateUT => "Insert_StateUT";
    public static string InsertCompany => "Insert_Company";
    public static string InsertLedger => "Insert_Ledger";
    public static string InsertGroup => "Insert_Group";
    public static string InsertAccountType => "Insert_AccountType";
    public static string InsertVoucher => "Insert_Voucher";
    public static string InsertFinancialYear => "Insert_FinancialYear";

    public static string InsertAccounting => "Insert_Accounting";
    public static string InsertAccountingDetail => "Insert_AccountingDetail";

    public static string InsertItem => "Insert_Item";
    public static string InsertItemCategory => "Insert_ItemCategory";
    public static string InsertItemType => "Insert_ItemType";
    public static string InsertManufacturer => "Insert_Manufacturer";
    public static string InsertTax => "Insert_Tax";
    public static string InsertItemStock => "Insert_ItemStock";

    public static string InsertPurchase => "Insert_Purchase";
    public static string InsertPurchaseDetail => "Insert_PurchaseDetail";
    public static string InsertPurchaseReturn => "Insert_PurchaseReturn";
    public static string InsertPurchaseReturnDetail => "Insert_PurchaseReturnDetail";

    public static string InsertItemIssue => "Insert_ItemIssue";
    public static string InsertItemIssueDetail => "Insert_ItemIssueDetail";

    public static string InsertVehicle => "Insert_Vehicle";
    public static string InsertVehicleModel => "Insert_VehicleModel";
    public static string InsertVehicleType => "Insert_VehicleType";

    public static string InsertGarage => "Insert_Garage";
    public static string InsertServiceType => "Insert_ServiceType";
    public static string InsertServiceSchedule => "Insert_ServiceSchedule";

    public static string InsertService => "Insert_Service";
    public static string InsertServiceDetail => "Insert_ServiceDetail";

    public static string DeleteItemStockById => "Delete_ItemStock_By_Id";
    public static string DeleteItemStockByTypeTransactionId => "Delete_ItemStock_By_Type_TransactionId";
}

public static class ViewNames
{
    public static string AccountingOverview => "Accounting_Overview";
    public static string AccountingLedgerOverview => "Accounting_Ledger_Overview";

    public static string ItemStockDetails => "ItemStockDetails";

    public static string PurchaseOverview => "Purchase_Overview";
    public static string PurchaseItemOverview => "Purchase_Item_Overview";
    public static string PurchaseReturnOverview => "PurchaseReturn_Overview";
    public static string PurchaseReturnItemOverview => "PurchaseReturn_Item_Overview";

    public static string ItemIssueOverview => "ItemIssue_Overview";
    public static string GarageIssueItemOverview => "GarageIssue_Item_Overview";
    public static string VehicleIssueItemOverview => "VehicleIssue_Item_Overview";

    public static string ServiceOverview => "Service_Overview";
    public static string GarageServiceItemOverview => "GarageService_Item_Overview";
    public static string VehicleServiceItemOverview => "VehicleService_Item_Overview";
}