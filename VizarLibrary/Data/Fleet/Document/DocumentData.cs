using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Document;

namespace VizarLibrary.Data.Fleet.Document;

public static class DocumentData
{
    private static async Task<int> InsertDocument(DocumentModel document) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertDocument, document)).FirstOrDefault();

    public static async Task<int> SaveTransaction(DocumentModel document)
    {
        var update = document.Id > 0;

        if (update)
        {
            var existingDocument = await CommonData.LoadTableDataById<DocumentModel>(TableNames.Document, document.Id);
            await FinancialYearData.ValidateFinancialYear(existingDocument.TransactionDateTime);
        }

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, document.FinancialYearId);
        await FinancialYearData.ValidateFinancialYear(document.TransactionDateTime);

        return await InsertDocument(document);
    }
}
