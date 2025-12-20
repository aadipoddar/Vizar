namespace VizarLibrary.Models.Fleet.Service;

public class ServiceScheduleModel
{
    public int Id { get; set; }
    public int ServiceTypeId { get; set; }
    public int VehicleTypeId { get; set; }
    public int IntervalDays { get; set; }
    public bool Status { get; set; }
}