namespace VizarLibrary.Models.Item;

public class ItemModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Code { get; set; }
	public int ItemTypeId { get; set; }
	public int ItemCategoryId { get; set; }
	public int? ManufacturerId { get; set; }
	public decimal Rate { get; set; }
	public int TaxId { get; set; }
	public string UnitOfMeasurement { get; set; }
	public decimal? ReorderLevel { get; set; }
	public string? Remarks { get; set; }
	public bool Status { get; set; }
}
