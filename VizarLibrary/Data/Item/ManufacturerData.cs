using VizarLibrary.DataAccess;
using VizarLibrary.Models.Item;

namespace VizarLibrary.Data.Item;

public static class ManufacturerData
{
	public static async Task<int> InsertManufacturer(ManufacturerModel manufacturer) =>
		(await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertManufacturer, manufacturer)).FirstOrDefault();
}
