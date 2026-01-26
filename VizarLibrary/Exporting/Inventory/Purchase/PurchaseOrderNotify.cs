using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Inventory.Purchase;

namespace VizarLibrary.Exporting.Inventory.Purchase;

internal static class PurchaseOrderNotify
{
    internal static async Task Notify(int purchaseOrderId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type != NotifyType.Created)
            await PurchaseOrderMail(purchaseOrderId, type, previousInvoice);
    }

    private static async Task PurchaseOrderMail(int purchaseOrderId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var purchaseOrder = await CommonData.LoadTableDataById<PurchaseOrderOverviewModel>(ViewNames.PurchaseOrderOverview, purchaseOrderId);

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Purchase Order",
            TransactionNo = purchaseOrder.TransactionNo,
            Action = type,
            LocationName = purchaseOrder.VendorName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = purchaseOrder.TransactionNo,
                ["Vendor"] = purchaseOrder.VendorName,
                ["Garage"] = purchaseOrder.GarageName,
                ["Transaction Date"] = purchaseOrder.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
                ["Total Items"] = purchaseOrder.TotalItems.ToString(),
                ["Total Quantity"] = purchaseOrder.TotalQuantity.FormatSmartDecimal(),
                ["Purchase No"] = purchaseOrder.PurchaseTransactionNo ?? "Not Linked",
                ["Purchase Date"] = purchaseOrder.PurchaseDateTime?.ToString("dd MMM yyyy") ?? "-",
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = purchaseOrder.LastModifiedByUserName ?? purchaseOrder.CreatedByName
            },
            Remarks = purchaseOrder.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await PurchaseOrderInvoiceExport.ExportInvoice(purchaseOrderId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await PurchaseOrderInvoiceExport.ExportInvoice(purchaseOrderId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
