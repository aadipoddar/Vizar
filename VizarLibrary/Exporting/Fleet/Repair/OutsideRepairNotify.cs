using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Repair;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class OutsideRepairNotify
{
    internal static async Task Notify(int outsideRepairId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type != NotifyType.Created)
            await OutsideRepairMail(outsideRepairId, type, previousInvoice);
    }

    private static async Task OutsideRepairMail(int outsideRepairId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var outsideRepair = await CommonData.LoadTableDataById<OutsideRepairOverviewModel>(ViewNames.OutsideRepairOverview, outsideRepairId);

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Outside Repair",
            TransactionNo = outsideRepair.TransactionNo,
            Action = type,
            LocationName = outsideRepair.VendorName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = outsideRepair.TransactionNo,
                ["Vendor"] = outsideRepair.VendorName,
                ["Vehicle"] = outsideRepair.VehicleCode,
                ["Transaction Date"] = outsideRepair.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
                ["Total Items"] = outsideRepair.TotalItems.ToString(),
                ["Total Quantity"] = outsideRepair.TotalQuantity.FormatSmartDecimal(),
                ["Total Amount"] = outsideRepair.TotalAmount.FormatIndianCurrency(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = outsideRepair.LastModifiedByUserName ?? outsideRepair.CreatedByName
            },
            Remarks = outsideRepair.Remarks
        };

        // Add Approved By if present
        if (!string.IsNullOrWhiteSpace(outsideRepair.ApprovedBy))
            emailData.Details.Add("Approved By", outsideRepair.ApprovedBy);

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await OutsideRepairInvoiceExport.ExportInvoice(outsideRepairId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await OutsideRepairInvoiceExport.ExportInvoice(outsideRepairId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
