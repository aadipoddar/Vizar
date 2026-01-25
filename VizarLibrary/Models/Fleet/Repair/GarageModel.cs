namespace VizarLibrary.Models.Fleet.Repair;

public class GarageModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool External { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}