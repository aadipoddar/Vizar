using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Repair;

namespace VizarLibrary.Exporting.Fleet.Repair;

public static class InsideRepairNotify
{
    internal static async Task Notify(int insideRepairId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type != NotifyType.Created)
            await InsideRepairMail(insideRepairId, type, previousInvoice);
    }

    private static async Task InsideRepairMail(int insideRepairId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var insideRepair = await CommonData.LoadTableDataById<InsideRepairOverviewModel>(ViewNames.InsideRepairOverview, insideRepairId);

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Inside Repair",
            TransactionNo = insideRepair.TransactionNo,
            Action = type,
            LocationName = insideRepair.GarageName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = insideRepair.TransactionNo,
                ["Garage"] = insideRepair.GarageName,
                ["Vehicle"] = insideRepair.VehicleCode,
                ["Transaction Date"] = insideRepair.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
                ["Total Items"] = insideRepair.TotalItems.ToString(),
                ["Total Quantity"] = insideRepair.TotalQuantity.FormatSmartDecimal(),
                ["Total Amount"] = insideRepair.TotalAmount.FormatIndianCurrency(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = insideRepair.LastModifiedByUserName ?? insideRepair.CreatedByName
            },
            Remarks = insideRepair.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await InsideRepairInvoiceExport.ExportInvoice(insideRepairId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await InsideRepairInvoiceExport.ExportInvoice(insideRepairId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
