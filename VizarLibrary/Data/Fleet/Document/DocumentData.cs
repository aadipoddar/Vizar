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
            var updateFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingDocument.FinancialYearId);
            if (updateFinancialYear is null || updateFinancialYear.Locked || updateFinancialYear.Status == false)
                throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");
        }

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, document.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

        return await InsertDocument(document);
    }
}
