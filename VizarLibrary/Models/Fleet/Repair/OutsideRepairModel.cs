namespace VizarLibrary.Models.Fleet.Repair;

public class OutsideRepairModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public int VendorId { get; set; }
    public int VehicleId { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public string? ApprovedBy { get; set; }
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

public class OutsideRepairDetailModel
{
    public int Id { get; set; }
    public int MasterId { get; set; }
    public string Job { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class OutsideRepairItemCartModel
{
    public string Job { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
    public string? Remarks { get; set; }
}

public class OutsideRepairOverviewModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public string FinancialYear { get; set; }

    public int VendorId { get; set; }
    public string VendorName { get; set; }

    public int VehicleId { get; set; }
    public string VehicleCode { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public string? ApprovedBy { get; set; }

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

public class OutsideRepairItemOverviewModel
{
    public string Job { get; set; }

    public int MasterId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; }
    public int VehicleId { get; set; }
    public string VehicleCode { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public string? ApprovedBy { get; set; }
    public string? OutsideRepairRemarks { get; set; }

    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }

    public string? Remarks { get; set; }
}