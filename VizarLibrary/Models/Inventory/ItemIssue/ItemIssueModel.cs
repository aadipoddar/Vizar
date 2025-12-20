namespace VizarLibrary.Models.Inventory.ItemIssue;

public class ItemIssueModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public int? GarageId { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedFromPlatform { get; set; }
    public bool Status { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedFromPlatform { get; set; }
}

public class ItemIssueDetailModel
{
    public int Id { get; set; }
    public int MasterId { get; set; }
    public int ItemId { get; set; }
    public int? VehicleId { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public string? IdentificationNo { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class ItemIssueItemCartModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleCode { get; set; }
    public string? VehicleShortCode { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public string? IdentificationNo { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
    public string? Remarks { get; set; }
}

public class ItemIssueOverviewModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public string FinancialYear { get; set; }

    public int? GarageId { get; set; }
    public string? GarageName { get; set; }

    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }

    public string Remarks { get; set; }
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

public class GarageIssueItemOverviewModel
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

    public int MasterId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int GarageId { get; set; }
    public string GarageName { get; set; }
    public string? ItemIssueRemarks { get; set; }

    public string? IdentificationNo { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }

    public string? Remarks { get; set; }
}

public class VehicleIssueItemOverviewModel
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

    public int MasterId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string? ItemIssueRemarks { get; set; }

    public int VehicleId { get; set; }
    public string VehicleCode { get; set; }
    public string VehicleShortCode { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public string? IdentificationNo { get; set; }
    public string UnitOfMeasurement { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }

    public string? Remarks { get; set; }

    public decimal? PreviousHour { get; set; }
    public decimal? PreviousKM { get; set; }
    public decimal? Average { get; set; }
}