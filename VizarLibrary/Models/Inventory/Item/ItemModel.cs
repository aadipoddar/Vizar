namespace VizarLibrary.Models.Inventory.Item;

public class ItemModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int ItemTypeId { get; set; }
    public int ItemCategoryId { get; set; }
    public string UnitOfMeasurement { get; set; }
    public string? PartNo { get; set; }
    public int? ManufacturerId { get; set; }
    public decimal Rate { get; set; }
    public int? TaxId { get; set; }
    public decimal? ReorderLevel { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class ItemCategoryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class ItemTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class ManufacturerModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}

public class TaxModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal CGST { get; set; }
    public decimal SGST { get; set; }
    public decimal IGST { get; set; }
    public bool Inclusive { get; set; }
    public bool Extra { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}