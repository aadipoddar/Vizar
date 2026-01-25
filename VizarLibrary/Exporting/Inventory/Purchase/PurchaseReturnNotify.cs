using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

internal static class PurchaseReturnNotify
{
    internal static async Task Notify(int purchaseReturnId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type != NotifyType.Created)
            await PurchaseReturnMail(purchaseReturnId, type, previousInvoice);
    }

    private static async Task PurchaseReturnMail(int purchaseReturnId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var purchaseReturn = await CommonData.LoadTableDataById<PurchaseReturnOverviewModel>(ViewNames.PurchaseReturnOverview, purchaseReturnId);

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Purchase Return",
            TransactionNo = purchaseReturn.TransactionNo,
            Action = type,
            LocationName = purchaseReturn.VendorName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = purchaseReturn.TransactionNo,
                ["Vendor"] = purchaseReturn.VendorName,
                ["Transaction Date"] = purchaseReturn.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
                ["Total Items"] = purchaseReturn.TotalItems.ToString(),
                ["Total Quantity"] = purchaseReturn.TotalQuantity.FormatSmartDecimal(),
                ["Base Total"] = purchaseReturn.BaseTotal.FormatIndianCurrency(),
                ["Total Amount"] = purchaseReturn.TotalAmount.FormatIndianCurrency(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = purchaseReturn.LastModifiedByUserName ?? purchaseReturn.CreatedByName
            },
            Remarks = purchaseReturn.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await PurchaseReturnInvoiceExport.ExportInvoice(purchaseReturnId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await PurchaseReturnInvoiceExport.ExportInvoice(purchaseReturnId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
