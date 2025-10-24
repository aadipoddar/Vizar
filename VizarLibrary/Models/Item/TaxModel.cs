namespace VizarLibrary.Models.Item;

public class TaxModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Code { get; set; }
	public decimal GST { get; set; }
	public bool Inclusive { get; set; }
	public bool Extra { get; set; }
	public string Remarks { get; set; }
	public bool Status { get; set; }
}
