using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Item;
using VizarLibrary.Models.Operations;

namespace VizarLibrary.Exporting.Inventory.Stock;

internal static class ItemStockAdjustmentNotify
{
    internal static async Task Notify(ItemStockModel stock, int userId, NotifyType type)
    {
        if (type == NotifyType.Deleted)
            await ItemStockAdjustmentMail(stock, userId);
    }

    private static async Task ItemStockAdjustmentMail(ItemStockModel stock, int userId)
    {
        var user = await CommonData.LoadTableDataById<UserModel>(TableNames.User, userId);
        var userName = user?.Name ?? "Unknown User";

        var rawMaterial = await CommonData.LoadTableDataById<ItemModel>(TableNames.Item, stock.ItemId);
        var rawMaterialName = rawMaterial?.Name ?? "Unknown Material";
        var rawMaterialCode = rawMaterial?.Code ?? "N/A";
        var uom = rawMaterial?.UnitOfMeasurement ?? "N/A";

        var emailData = new MailingUtil.TransactionEmailData
        {
            TransactionType = "Raw Material Stock Adjustment",
            TransactionNo = stock.TransactionNo,
            Action = NotifyType.Deleted,
            LocationName = "Main Location",
            Details = new Dictionary<string, string>
            {
                ["Transaction Number"] = stock.TransactionNo ?? "N/A",
                ["Transaction Date"] = stock.TransactionDateTime.ToString("dd MMM yyyy hh:mm tt"),
                ["Raw Material"] = rawMaterialName,
                ["Code"] = rawMaterialCode,
                ["Unit of Measurement"] = uom,
                ["Quantity Deleted"] = stock.Quantity.FormatSmartDecimal(),
                ["Deleted By"] = userName
            },
            Remarks = null
        };

        await MailingUtil.SendTransactionEmail(emailData);
    }
}
