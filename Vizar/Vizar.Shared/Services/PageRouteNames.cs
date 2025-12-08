namespace Vizar.Shared.Services;

public static class PageRouteNames
{
	public static string Dashboard => "/";
	public static string Login => "/login";
	public static string LoginWithCode => "/login-with-code";
	public static string LoginWithCodeRedirect => "login-with-code-redirect"; // Do not put leading slash

	public static string AccountsDashboard => "/accounts";
	public static string FinancialAccounting => "/accounts/financial-accounting";

	public static string InventoryDashboard => "/inventory";
	public static string Purchase => "/inventory/purchase";
	public static string PurchaseReturn => "/inventory/purchase-return";
	public static string ProductStockAdjustment => "/inventory/item-stock-adjustment";

	public static string ReportDashboard => "/report";
	public static string ReportFinancialAccounting => "/report/financial-accounting";
	public static string ReportAccountingLedger => "/report/accounting-ledger";
	public static string ReportTrialBalance => "/report/trial-balance";
	public static string ReportPurchase => "/report/purchase";
	public static string ReportPurchaseReturn => "/report/purchase-return";
	public static string ReportPurchaseItem => "/report/purchase-item";
	public static string ReportPurchaseReturnItem => "/report/purchase-return-item";

	public static string AdminDashboard => "/admin";
	public static string AdminProduct => "/admin/item";
	public static string AdminUser => "/admin/user";
	public static string AdminTax => "/admin/tax";
	public static string AdminCompany => "/admin/company";
	public static string AdminLedger => "/admin/ledger";
	public static string AdminVoucher => "/admin/voucher";
	public static string AdminGroup => "/admin/group";
	public static string AdminAccountType => "/admin/account-type";
	public static string AdminFinancialYear => "/admin/financial-year";
	public static string AdminStateUT => "/admin/state-ut";
	public static string AdminSettings => "/admin/settings";
}
