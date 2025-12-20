namespace VizarLibrary.Models.Fleet.Service;

public class ServiceModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public int GarageId { get; set; }
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

public class ServiceDetailModel
{
    public int Id { get; set; }
    public int MasterId { get; set; }
    public int ServiceTypeId { get; set; }
    public int VehicleId { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class ServiceItemCartModel
{
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; }
    public int VehicleId { get; set; }
    public string VehicleCode { get; set; }
    public string VehicleShortCode { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
    public string? Remarks { get; set; }
}

public class ServiceOverviewModel
{
    public int Id { get; set; }
    public string TransactionNo { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int FinancialYearId { get; set; }
    public string FinancialYear { get; set; }

    public int GarageId { get; set; }
    public string GarageName { get; set; }

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

public class GarageServiceItemOverviewModel
{
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; }
    public string ServiceTypeCode { get; set; }

    public int MasterId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int GarageId { get; set; }
    public string GarageName { get; set; }
    public int VehicleId { get; set; }
    public string VehicleCode { get; set; }
    public string? ServiceRemarks { get; set; }

    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }

    public string? Remarks { get; set; }
}

public class VehicleServiceItemOverviewModel
{
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; }
    public string ServiceTypeCode { get; set; }

    public int MasterId { get; set; }
    public string TransactionNo { get; set; }
    public DateTime TransactionDateTime { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public int GarageId { get; set; }
    public string GarageName { get; set; }
    public string? ServiceRemarks { get; set; }

    public int VehicleId { get; set; }
    public string VehicleCode { get; set; }
    public string VehicleShortCode { get; set; }
    public decimal? CurrentHour { get; set; }
    public decimal? CurrentKM { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }

    public string? Remarks { get; set; }

    public decimal? PreviousHour { get; set; }
    public decimal? PreviousKM { get; set; }
    public decimal? Average { get; set; }

    public int? IntervalDays { get; set; }
    public DateTime? NextDueDate { get; set; }
}