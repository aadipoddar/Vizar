namespace VizarLibrary.Models.Operations;

public class SettingsModel
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
}

public static class SettingsKeys
{
    public static string EnableLoginWithCode => "EnableLoginWithCode";
    public static string MaxLoginAttempts => "MaxLoginAttempts";
    public static string EnableUsersToResetPassword => "EnableUsersToResetPassword";
    public static string CodeResendLimit => "CodeResendLimit";
    public static string CodeExpiryMinutes => "CodeExpiryMinutes";

    public static string LedgerCodePrefix => "LedgerCodePrefix";

    public static string ItemCodePrefix => "ItemCodePrefix";
    public static string ItemTypeCodePrefix => "ItemTypeCodePrefix";
    public static string ItemCategoryCodePrefix => "ItemCategoryCodePrefix";
    public static string DocumentTypeCodePrefix => "DocumentTypeCodePrefix";
    public static string ServiceTypeCodePrefix => "ServiceTypeCodePrefix";
    public static string ManufacturerCodePrefix => "ManufacturerCodePrefix";
    public static string VehicleTypeCodePrefix => "VehicleTypeCodePrefix";

    public static string FinancialAccountingTransactionPrefix => "FinancialAccountingTransactionPrefix";

    public static string PurchaseTransactionPrefix => "PurchaseTransactionPrefix";
    public static string PurchaseReturnTransactionPrefix => "PurchaseReturnTransactionPrefix";
    public static string ItemIssueTransactionPrefix => "ItemIssueTransactionPrefix";
    public static string ItemStockAdjustmentTransactionPrefix => "ItemStockAdjustmentTransactionPrefix";

    public static string ServiceTransactionPrefix => "ServiceTransactionPrefix";

    public static string UpdateItemMasterRateOnPurchase => "UpdateItemMasterRateOnPurchase";
    public static string UpdateItemMasterUOMOnPurchase => "UpdateItemMasterUOMOnPurchase";

    public static string PrimaryCompanyLinkingId => "PrimaryCompanyLinkingId";

    public static string PurchaseVoucherId => "PurchaseVoucherId";
    public static string PurchaseReturnVoucherId => "PurchaseReturnVoucherId";
    public static string PurchaseLedgerId => "PurchaseLedgerId";
    public static string CashLedgerId => "CashLedgerId";
    public static string GSTLedgerId => "GSTLedgerId";

    public static string AutoRefreshReportTimer => "AutoRefreshReportTimer";
}