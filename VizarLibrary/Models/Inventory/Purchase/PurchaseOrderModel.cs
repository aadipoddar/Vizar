namespace VizarLibrary.Models.Inventory.Purchase;

public class PurchaseOrderModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public int VendorId { get; set; }
    public int GarageId { get; set; }
    public int? PurchaseId { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedFromPlatform { get; set; }
    public bool Status { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedFromPlatform { get; set; }
}

public class PurchaseOrderDetailModel
{
    public int Id { get; set; }
    public int MasterId { get; set; }
    public int ItemId { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class PurchaseOrderItemCartModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public string? Remarks { get; set; }
}

public class PurchaseOrderOverviewModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }

    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public int GarageId { get; set; }
    public string GarageName { get; set; }

    public int? PurchaseId { get; set; }
    public string? PurchaseTransactionNo { get; set; }
    public DateTime? PurchaseDateTime { get; set; }
    public DateTime? PurchaseReceiveDateTime { get; set; }

    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public string FinancialYear { get; set; }

    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }

    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedFromPlatform { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByUserName { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedFromPlatform { get; set; }
    public bool Status { get; set; }
}

public class PurchaseOrderItemOverviewModel
{
    public int Id { get; set; }
    public string ItemName { get; set; }
    public string ItemCode { get; set; }
    public int ItemTypeId { get; set; }
    public string ItemTypeName { get; set; }
    public int ItemCategoryId { get; set; }
    public string ItemCategoryName { get; set; }
    public int ManufacturerId { get; set; }
    public string ManufacturerName { get; set; }

    public int MasterId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public int GarageId { get; set; }
    public string GarageName { get; set; }

    public int? PurchaseId { get; set; }
    public string? PurchaseTransactionNo { get; set; }
    public DateTime? PurchaseDateTime { get; set; }
    public DateTime? PurchaseReceiveDateTime { get; set; }
    public decimal? PurchaseQuantity { get; set; }
    public string? PurchaseOrderRemarks { get; set; }

    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public string? Remarks { get; set; }
}