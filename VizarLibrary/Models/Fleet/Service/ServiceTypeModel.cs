namespace VizarLibrary.Models.Fleet.Service;

public class ServiceTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal Rate { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}
