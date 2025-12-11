using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Accounts.FinancialAccounting;
using VizarLibrary.Models.Accounts.Masters;

namespace VizarLibrary.Exporting.Accounts.FinancialAccounting;

/// <summary>
/// Convert Accounting voucher data to Invoice Excel format
/// </summary>
public static class AccountingInvoiceExcelExport
{
	/// <summary>
	/// Export Accounting voucher as a professional accounting voucher Excel (automatically loads ledger names)
	/// </summary>
	/// <param name="accountingHeader">Accounting header data</param>
	/// <param name="accountingDetails">Accounting detail line items (ledger entries)</param>
	/// <param name="company">Company information</param>
	/// <param name="voucher">Voucher type information</param>
	/// <param name="logoPath">Optional: Path to company logo</param>
	/// <param name="invoiceType">Type of document (JOURNAL VOUCHER, PAYMENT VOUCHER, etc.)</param>
	/// <returns>MemoryStream containing the Excel file</returns>
	public static async Task<MemoryStream> ExportAccountingInvoice(
		AccountingModel accountingHeader,
		List<AccountingDetailModel> accountingDetails,
		CompanyModel company,
		VoucherModel voucher,
		string logoPath = null,
		string invoiceType = "ACCOUNTING VOUCHER")
	{
		// Load all ledgers to get names
		var allLedgers = await CommonData.LoadTableData<LedgerModel>(TableNames.Ledger);

		// Map to cart items with actual ledger names
		var cartItems = accountingDetails.Select(detail =>
		{
			var ledger = allLedgers.FirstOrDefault(l => l.Id == detail.LedgerId);
			string ledgerName = ledger?.Name ?? $"Ledger #{detail.LedgerId}";

			return new AccountingItemCartModel
			{
				LedgerId = detail.LedgerId,
				LedgerName = ledgerName,
				ReferenceNo = detail.ReferenceNo,
				ReferenceType = detail.ReferenceType,
				Debit = detail.Debit,
				Credit = detail.Credit,
				Remarks = detail.Remarks
			};
		}).ToList();

		// Define column settings with # column first
		var columnSettings = new List<ExcelInvoiceExportUtil.InvoiceColumnSetting>
		{
			new("#", "#", 5, Syncfusion.XlsIO.ExcelHAlign.HAlignCenter),
			new("LedgerName", "Ledger", 35, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
			new("ReferenceNo", "Ref No", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft),
			new("Debit", "Debit", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new("Credit", "Credit", 15, Syncfusion.XlsIO.ExcelHAlign.HAlignRight, "#,##0.00"),
			new("Remarks", "Remarks", 25, Syncfusion.XlsIO.ExcelHAlign.HAlignLeft)
		};

		// Calculate totals
		decimal totalDebit = cartItems.Sum(i => i.Debit ?? 0);
		decimal totalCredit = cartItems.Sum(i => i.Credit ?? 0);
		decimal difference = totalDebit - totalCredit;

		// Define summary fields
		var summaryFields = new Dictionary<string, string>
		{
			{ "Total Debit:", totalDebit.ToString() },
			{ "Total Credit:", totalCredit.ToString() },
			{ "Difference:", difference.ToString() }
		};

		// Map invoice header data
		var invoiceData = new ExcelInvoiceExportUtil.InvoiceData
		{
			TransactionNo = accountingHeader.TransactionNo,
			TransactionDateTime = accountingHeader.TransactionDateTime,
			ReferenceTransactionNo = accountingHeader.ReferenceNo,
			TotalAmount = Math.Max(accountingHeader.TotalDebitAmount, accountingHeader.TotalCreditAmount),
			Remarks = accountingHeader.Remarks,
			Status = accountingHeader.Status,
			PaymentModes = null // No payment modes for accounting vouchers
		};

		// Use voucher name as invoice type
		string voucherInvoiceType = !string.IsNullOrWhiteSpace(voucher?.Name)
			? $"{voucher.Name.ToUpper()}"
			: invoiceType;

		// Generate voucher Excel with generic method
		return await ExcelInvoiceExportUtil.ExportInvoiceToExcel(
			invoiceData,
			cartItems,
			company,
			null, // No billTo for accounting vouchers
			logoPath,
			voucherInvoiceType,
			columnSettings,
			null,
			summaryFields
		);
	}
}
