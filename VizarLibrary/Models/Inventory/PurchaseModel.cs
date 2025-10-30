namespace VizarLibrary.Models.Inventory;

public class PurchaseModel
{
	public int Id { get; set; }
	public string TransactionNo { get; set; }
	public int CompanyId { get; set; }
	public int PartyId { get; set; }
	public DateTime TransactionDateTime { get; set; }
	public int FinancialYearId { get; set; }
	public decimal ItemsTotalAmount { get; set; }
	public decimal OtherChargesPercent { get; set; }
	public decimal OtherChargesAmount { get; set; }
	public decimal CashDiscountPercent { get; set; }
	public decimal CashDiscountAmount { get; set; }
	public decimal RoundOffAmount { get; set; }
	public decimal TotalAmount { get; set; }
	public string? Remarks { get; set; }
	public string? DocumentUrl { get; set; }
	public int UserId { get; set; }
	public DateTime CreatedAt { get; set; }
	public string CreatedFromPlatform { get; set; }
	public bool Status { get; set; }
	public int? LastModifiedBy { get; set; }
	public DateTime? LastModifiedAt { get; set; }
	public string? LastModifiedFromPlatform { get; set; }
}

public class PurchaseDetailModel
{
	public int Id { get; set; }
	public int PurchaseId { get; set; }
	public int ItemId { get; set; }
	public string? IdentificationNo { get; set; }
	public decimal Quantity { get; set; }
	public string UnitOfMeasurement { get; set; }
	public decimal Rate { get; set; }
	public decimal BaseTotal { get; set; }
	public decimal DiscountPercent { get; set; }
	public decimal DiscountAmount { get; set; }
	public decimal AfterDiscount { get; set; }
	public decimal CGSTPercent { get; set; }
	public decimal CGSTAmount { get; set; }
	public decimal SGSTPercent { get; set; }
	public decimal SGSTAmount { get; set; }
	public decimal IGSTPercent { get; set; }
	public decimal IGSTAmount { get; set; }
	public decimal TotalTaxAmount { get; set; }
	public bool InclusiveTax { get; set; }
	public decimal Total { get; set; }
	public decimal NetRate { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}

public class PurchaseItemCartModel
{
	public int ItemId { get; set; }
	public string ItemName { get; set; }
	public string? IdentificationNo { get; set; }
	public decimal Quantity { get; set; }
	public string UnitOfMeasurement { get; set; }
	public decimal Rate { get; set; }
	public decimal BaseTotal { get; set; }
	public decimal DiscountPercent { get; set; }
	public decimal DiscountAmount { get; set; }
	public decimal AfterDiscount { get; set; }
	public decimal CGSTPercent { get; set; }
	public decimal CGSTAmount { get; set; }
	public decimal SGSTPercent { get; set; }
	public decimal SGSTAmount { get; set; }
	public decimal IGSTPercent { get; set; }
	public decimal IGSTAmount { get; set; }
	public decimal TotalTaxAmount { get; set; }
	public bool InclusiveTax { get; set; }
	public decimal Total { get; set; }
	public decimal NetRate { get; set; }
	public string? Remarks { get; set; }
}