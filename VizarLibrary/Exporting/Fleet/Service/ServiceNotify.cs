using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Exporting.Fleet.Service;

internal static class ServiceNotify
{
    internal static async Task Notify(int serviceId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        if (type != NotifyType.Created)
            await ServiceMail(serviceId, type, previousInvoice);
    }

    private static async Task ServiceMail(int serviceId, NotifyType type, (MemoryStream, string)? previousInvoice = null)
    {
        var service = await CommonData.LoadTableDataById<ServiceOverviewModel>(ViewNames.ServiceOverview, serviceId);

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Service",
            TransactionNo = service.TransactionNo,
            Action = type,
            LocationName = service.GarageName,
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = service.TransactionNo,
                ["Garage"] = service.GarageName,
                ["Transaction Date"] = service.TransactionDateTime.ToString("dd MMM yyyy, hh:mm tt"),
                ["Total Items"] = service.TotalItems.ToString(),
                ["Total Quantity"] = service.TotalQuantity.FormatSmartDecimal(),
                ["Total Amount"] = service.TotalAmount.FormatIndianCurrency(),
                [type == NotifyType.Deleted ? "Deleted By" : type == NotifyType.Updated ? "Updated By" : "Modified By"] = service.LastModifiedByUserName ?? service.CreatedByName
            },
            Remarks = service.Remarks
        };

        // For update emails, include before and after invoices
        if (type == NotifyType.Updated && previousInvoice.HasValue)
        {
            var (afterStream, afterFileName) = await ServiceInvoiceExport.ExportInvoice(serviceId, InvoiceExportType.PDF);

            // Rename files to make it clear which is which
            var beforeFileName = $"BEFORE_{previousInvoice.Value.Item2}";
            var afterFileNameWithPrefix = $"AFTER_{afterFileName}";

            emailData.BeforeAttachment = (previousInvoice.Value.Item1, beforeFileName);
            emailData.AfterAttachment = (afterStream, afterFileNameWithPrefix);
        }
        else
        {
            // For delete/recover, just attach the current invoice
            var (pdfStream, pdfFileName) = await ServiceInvoiceExport.ExportInvoice(serviceId, InvoiceExportType.PDF);
            emailData.Attachments = new Dictionary<MemoryStream, string> { { pdfStream, pdfFileName } };
        }

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
