namespace VizarLibrary.Models.Common;

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

	public static string PrimaryCompanyLinkingId => "PrimaryCompanyLinkingId";

	public static string LedgerCodePrefix => "LedgerCodePrefix";
	public static string ItemCategoryCodePrefix => "ItemCategoryCodePrefix";
	public static string ItemTypeCodePrefix => "ItemTypeCodePrefix";
	public static string ItemCodePrefix => "ItemCodePrefix";
	public static string ManufacturerCodePrefix => "ManufacturerCodePrefix";
	public static string PurchaseTransactionPrefix => "PurchaseTransactionPrefix";

	public static string UpdateItemMasterRateOnPurchase => "UpdateItemMasterRateOnPurchase";
	public static string UpdateItemMasterUOMOnPurchase => "UpdateItemMasterUOMOnPurchase";
}