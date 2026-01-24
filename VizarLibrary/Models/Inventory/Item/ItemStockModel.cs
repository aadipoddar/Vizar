namespace VizarLibrary.Models.Inventory.Item;

public class ItemStockModel
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string? IdentificationNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal? NetRate { get; set; }
    public string Type { get; set; }
    public int? TransactionId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
}

public enum StockType
{
    Purchase,
    PurchaseReturn,
    ItemIssue,
    Adjustment
}

public class ItemStockDetailsModel
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }
    public int ItemTypeId { get; set; }
    public string ItemTypeName { get; set; }
    public int ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; }
    public int ManufacturerId { get; set; }
    public string ManufacturerName { get; set; }
    public string? IdentificationNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal? NetRate { get; set; }
    public string Type { get; set; }
    public int? TransactionId { get; set; }
    public string TransactionNo { get; set; }
    public DateOnly TransactionDateTime { get; set; }
}

public class ItemStockSummaryModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }
    public int ItemTypeId { get; set; }
    public string ItemTypeName { get; set; }
    public int ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; }
    public int ManufacturerId { get; set; }
    public string ManufacturerName { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal OpeningStock { get; set; }
    public decimal PurchaseStock { get; set; }
    public decimal SaleStock { get; set; }
    public decimal MonthlyStock { get; set; }
    public decimal ClosingStock { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal Rate { get; set; }
    public decimal ClosingValue { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal LastPurchasePrice { get; set; }
    public decimal WeightedAverageValue { get; set; }
    public decimal LastPurchaseValue { get; set; }
}

public class ItemStockAdjustmentCartModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string? IdentificationNo { get; set; }
    public decimal Stock { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
}