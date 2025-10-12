namespace VizarLibrary.Models.Common;

public class UserModel
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string UserId { get; set; }
	public string Password { get; set; }
	public string? Phone { get; set; }
	public string? Email { get; set; }
	public bool Inventory { get; set; }
	public bool Admin { get; set; }
	public bool Status { get; set; }
}
