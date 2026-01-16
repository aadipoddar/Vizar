using VizarLibrary.DataAccess;
using VizarLibrary.Models.Fleet.Document;

namespace VizarLibrary.Data.Fleet.Masters;

public static class DocumentTypeData
{
    public static async Task<int> InsertDocumentType(DocumentTypeModel documentType) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertDocumentType, documentType)).FirstOrDefault();
}
