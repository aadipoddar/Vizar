using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.ItemIssue;

namespace VizarLibrary.Exporting.Inventory.ItemIssue;

internal static class ItemIssueNotify
{
    internal static async Task Notify(int itemIssueId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type != NotifyType.Created)
            await ItemIssueMail(itemIssueId, type, previousInvoice);
    }

    private static async Task ItemIssueMail(int itemIssueId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var itemIssue = await CommonData.LoadTableDataById<ItemIssueOverviewModel>(ViewNames.ItemIssueOverview, itemIssueId);

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Item Issue",
            TransactionNo = itemIssue.TransactionNo,
            Action = type,
            LocationName = itemIssue.GarageName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = itemIssue.TransactionNo,
                ["Garage"] = itemIssue.GarageName,
                ["Transaction Date"] = itemIssue.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
                ["Total Items"] = itemIssue.TotalItems.ToString(),
                ["Total Quantity"] = itemIssue.TotalQuantity.FormatSmartDecimal(),
                ["Total Amount"] = itemIssue.TotalAmount.FormatIndianCurrency(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = itemIssue.LastModifiedByUserName ?? itemIssue.CreatedByName
            },
            Remarks = itemIssue.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await ItemIssueInvoiceExport.ExportInvoice(itemIssueId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await ItemIssueInvoiceExport.ExportInvoice(itemIssueId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
