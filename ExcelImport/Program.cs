using OfficeOpenXml;

using VizarLibrary.Data;
using VizarLibrary.Data.Accounts;
using VizarLibrary.Data.Common;
using VizarLibrary.Data.Item;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Item;

FileInfo fileInfo = new(@"C:\Others\categories.xlsx");
ExcelPackage.License.SetNonCommercialPersonal("AadiSoft");
using var package = new ExcelPackage(fileInfo);
await package.LoadAsync(fileInfo);
var worksheet = package.Workbook.Worksheets[0];

//await InsertItemCategory(worksheet);
//await InsertItemType(worksheet);
//await InsertManufacturer(worksheet);
//await InsertItem(worksheet);
//await InsertLedger(worksheet);

Console.WriteLine("Completed");
Console.ReadLine();

static async Task InsertItemCategory(ExcelWorksheet worksheet)
{
	int row = 2;

	while (worksheet.Cells[row, 1].Value != null)
	{
		var name = worksheet.Cells[row, 2].Value.ToString();
		var remarks = worksheet.Cells[row, 3].Value.ToString();
		var code = await GenerateCodes.GenerateItemCategoryCode();

		if (string.IsNullOrWhiteSpace(code) ||
			string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(remarks))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		code = code.Replace(" ", "");

		Console.WriteLine("Inserting New Product: " + name + " and code " + code);
		try
		{
			await ItemCategoryData.InsertItemCategory(new()
			{
				Id = 0,
				Code = code,
				Name = name,
				Remarks = remarks,
				Status = true
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error Inserting Row = " + row + " Error: " + ex.Message);
		}

		row++;
	}
}

static async Task InsertItemType(ExcelWorksheet worksheet)
{
	int row = 2;

	while (worksheet.Cells[row, 1].Value != null)
	{
		var name = worksheet.Cells[row, 2].Value.ToString();
		var remarks = worksheet.Cells[row, 3].Value.ToString();
		var code = await GenerateCodes.GenerateItemTypeCode();

		if (string.IsNullOrWhiteSpace(code) ||
			string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(remarks))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		code = code.Replace(" ", "");

		Console.WriteLine("Inserting New Product: " + name + " and code " + code);
		try
		{
			await ItemTypeData.InsertItemType(new()
			{
				Id = 0,
				Code = code,
				Name = name,
				Remarks = remarks,
				Status = true
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error Inserting Row = " + row + " Error: " + ex.Message);
		}

		row++;
	}
}

static async Task InsertManufacturer(ExcelWorksheet worksheet)
{
	int row = 2;

	while (worksheet.Cells[row, 1].Value != null)
	{
		var name = worksheet.Cells[row, 2].Value.ToString();
		var remarks = worksheet.Cells[row, 3].Value.ToString();
		var code = await GenerateCodes.GenerateManufactureCode();

		if (string.IsNullOrWhiteSpace(code) ||
			string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(remarks))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		code = code.Replace(" ", "");

		Console.WriteLine("Inserting New Product: " + name + " and code " + code);
		try
		{
			await ManufacturerData.InsertManufacturer(new()
			{
				Id = 0,
				Code = code,
				Name = name,
				Remarks = remarks,
				Status = true
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error Inserting Row = " + row + " Error: " + ex.Message);
		}

		row++;
	}
}

static async Task InsertItem(ExcelWorksheet worksheet)
{
	int row = 2;

	while (worksheet.Cells[row, 1].Value != null)
	{
		var name = worksheet.Cells[row, 2].Value.ToString();
		var remarks = worksheet.Cells[row, 3].Value.ToString();
		var rate = worksheet.Cells[row, 4].Value.ToString();
		var reorderLevel = worksheet.Cells[row, 5].Value;
		var code = await GenerateCodes.GenerateItemCode();

		if (string.IsNullOrWhiteSpace(code) ||
			string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(remarks))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		var allTaxes = await CommonData.LoadTableDataByStatus<TaxModel>(TableNames.Tax);
		var manufacturers = await CommonData.LoadTableDataByStatus<ManufacturerModel>(TableNames.Manufacturer);
		var itemCategories = await CommonData.LoadTableDataByStatus<ItemCategoryModel>(TableNames.ItemCategory);
		var itemTypes = await CommonData.LoadTableDataByStatus<ItemTypeModel>(TableNames.ItemType);

		var taxId = Random.Shared.Next(0, allTaxes.Max(t => t.Id) + 1);
		var manufacurerId = Random.Shared.Next(0, manufacturers.Max(m => m.Id) + 1);
		var itemCategoryId = Random.Shared.Next(0, itemCategories.Max(ic => ic.Id) + 1);
		var itemTypeId = Random.Shared.Next(0, itemTypes.Max(it => it.Id) + 1);

		Console.WriteLine("Inserting New Product: " + name + " and code " + code);
		try
		{
			await ItemData.InsertItem(new()
			{
				Id = 0,
				Code = code,
				Name = name,
				ItemCategory = itemCategoryId,
				ItemType = itemTypeId,
				ManufacturerId = manufacurerId,
				TaxId = taxId,
				Rate = decimal.Parse(rate),
				ReorderLevel = string.IsNullOrWhiteSpace(reorderLevel?.ToString()) ? null : decimal.Parse(reorderLevel.ToString()),
				UnitOfMeasurement = "KG",
				Remarks = remarks,
				Status = true
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error Inserting Row = " + row + " Error: " + ex.Message);
		}

		row++;
	}
}

static async Task InsertLedger(ExcelWorksheet worksheet)
{
	int row = 2;

	while (worksheet.Cells[row, 1].Value != null)
	{
		var name = worksheet.Cells[row, 2].Value.ToString();
		var remarks = worksheet.Cells[row, 3].Value.ToString();
		var GSTNo = worksheet.Cells[row, 4].Value;
		var alias = worksheet.Cells[row, 5].Value;
		var phone = worksheet.Cells[row, 6].Value;
		var email = worksheet.Cells[row, 7].Value;
		var address = worksheet.Cells[row, 8].Value;
		var panNo = worksheet.Cells[row, 9].Value;
		var code = await GenerateCodes.GenerateLedgerCode();

		var state = Random.Shared.Next(1, 3);

		if (string.IsNullOrWhiteSpace(code) ||
			string.IsNullOrWhiteSpace(name) ||
			string.IsNullOrWhiteSpace(remarks))
		{
			Console.WriteLine("Not Inserted Row = " + row);
			continue;
		}

		Console.WriteLine("Inserting New Product: " + name + " and code " + code);
		try
		{
			await LedgerData.InsertLedger(new()
			{
				Id = 0,
				Code = code,
				Name = name,
				AccountTypeId = 3,
				GroupId = 1,
				CINNo = null,
				Email = email?.ToString(),
				PANNo = panNo?.ToString(),
				StateUTId = state,
				Address = address?.ToString(),
				GSTNo = GSTNo?.ToString(),
				Alias = alias?.ToString(),
				Phone = phone?.ToString(),
				Remarks = remarks,
				Status = true
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error Inserting Row = " + row + " Error: " + ex.Message);
		}

		row++;
	}
}