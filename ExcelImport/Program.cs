using OfficeOpenXml;

using VizarLibrary.Data.Common;
using VizarLibrary.Data.Fleet.Vehicle;

FileInfo fileInfo = new(@"C:\Others\vehicle.xlsx");

ExcelPackage.License.SetNonCommercialPersonal("AadiSoft");

using var package = new ExcelPackage(fileInfo);

await package.LoadAsync(fileInfo);

var worksheet1 = package.Workbook.Worksheets[0];

await InsertVehicles(worksheet1);

Console.WriteLine("Finished importing Items.");
Console.ReadLine();

static async Task InsertVehicles(ExcelWorksheet worksheet)
{
    int row = 1;

    while (worksheet.Cells[row, 2].Value != null)
    {
        var model = worksheet.Cells[row, 1].Value.ToString();
        var code = worksheet.Cells[row, 2].Value?.ToString();
        var chasis = worksheet.Cells[row, 3].Value?.ToString();
        var engine = worksheet.Cells[row, 4].Value?.ToString();

        code = code.RemoveSpace();

        // last 4 digits
        var shortCode = code.Length > 4 ? code[^4..] : code;

        if (string.IsNullOrWhiteSpace(code))
        {
            Console.WriteLine("Not Inserted Row = " + row);
            continue;
        }

        Console.WriteLine("Inserting New Vehicle: " + code);

        try
        {
            await VehicleData.InsertVehicle(new()
            {
                Code = code,
                ShortCode = shortCode,
                ChasisCode = chasis,
                EngineCode = engine,
                PurchaseDate = DateTime.Now,
                VehicleTypeId = 1,
                VehicleModelId = int.Parse(model),
                Status = true
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Inserting Code = " + code + " Error: " + ex.Message);
        }

        row++;
    }
}