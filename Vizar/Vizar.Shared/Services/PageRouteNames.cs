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
    public static string ItemIssue => "/inventory/item-issue";
    public static string ItemStockAdjustment => "/inventory/item-stock-adjustment";

    public static string FleetDashboard => "/fleet";
    public static string Service => "/fleet/service";
    public static string Document => "/fleet/document";

    public static string ReportDashboard => "/report";
    public static string ReportFinancialAccounting => "/report/financial-accounting";
    public static string ReportAccountingLedger => "/report/accounting-ledger";
    public static string ReportTrialBalance => "/report/trial-balance";
    public static string ReportProfitAndLoss => "/report/profit-and-loss";
    public static string ReportBalanceSheet => "/report/balance-sheet";
    public static string ReportPurchase => "/report/purchase";
    public static string ReportPurchaseReturn => "/report/purchase-return";
    public static string ReportPurchaseItem => "/report/purchase-item";
    public static string ReportPurchaseReturnItem => "/report/purchase-return-item";
    public static string ReportItemIssue => "/report/item-issue";
    public static string ReportGarageIssueItem => "/report/garage-issue-item";
    public static string ReportVehicleIssueItem => "/report/vehicle-issue-item";
    public static string ReportItemStock => "/report/item-stock";
    public static string ReportService => "/report/service";
    public static string ReportGarageServiceItem => "/report/garage-service-item";
    public static string ReportVehicleServiceItem => "/report/vehicle-service-item";

    public static string AdminDashboard => "/admin";
    public static string AdminUser => "/admin/user";
    public static string AdminCompany => "/admin/company";
    public static string AdminLedger => "/admin/ledger";
    public static string AdminVoucher => "/admin/voucher";
    public static string AdminGroup => "/admin/group";
    public static string AdminAccountType => "/admin/account-type";
    public static string AdminFinancialYear => "/admin/financial-year";
    public static string AdminStateUT => "/admin/state-ut";
    public static string AdminSettings => "/admin/settings";
    public static string AdminItem => "/admin/item";
    public static string AdminItemCategory => "/admin/item-category";
    public static string AdminItemType => "/admin/item-type";
    public static string AdminManufacturer => "/admin/manufacturer";
    public static string AdminTax => "/admin/tax";
    public static string AdminDocumentType => "/admin/document-type";
    public static string AdminServiceType => "/admin/service-type";
    public static string AdminGarage => "/admin/garage";
    public static string AdminVehicle => "/admin/vehicle";
    public static string AdminVehicleModel => "/admin/vehicle-model";
    public static string AdminVehicleType => "/admin/vehicle-type";
}

