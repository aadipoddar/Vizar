using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Models.Accounts.Masters;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Data.Fleet.Service;

public static class ServiceData
{
    private static async Task<int> InsertService(ServiceModel service) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertService, service)).FirstOrDefault();

    private static async Task<int> InsertServiceDetail(ServiceDetailModel serviceDetail) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertServiceDetail, serviceDetail)).FirstOrDefault();

    public static async Task<VehicleServiceItemOverviewModel> LoadLastVehicleServiceItemByVehicleServiceTypeDate(int VehicleId, int ServiceTypeId, DateTime TransactionDateTime) =>
        (await SqlDataAccess.LoadData<VehicleServiceItemOverviewModel, dynamic>(StoredProcedureNames.LoadLastVehicleServiceItemByVehicleServiceTypeDate, new { VehicleId, ServiceTypeId, TransactionDateTime })).FirstOrDefault();

    public static async Task DeleteTransaction(ServiceModel transaction)
    {
        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, transaction.FinancialYearId);
        if (financialYear is null || financialYear.Locked || !financialYear.Status)
            throw new InvalidOperationException("Cannot delete transaction as the financial year is locked.");

        transaction.Status = false;
        await InsertService(transaction);
    }

    public static async Task RecoverTransaction(ServiceModel transaction)
    {
        var transactionDetails = await CommonData.LoadTableDataByMasterId<ServiceDetailModel>(TableNames.ServiceDetail, transaction.Id);
        List<ServiceItemCartModel> itemIssueItemCarts = [];

        itemIssueItemCarts.AddRange(transactionDetails.Select(item => new ServiceItemCartModel()
        {
            ServiceTypeId = item.ServiceTypeId,
            ServiceTypeName = "",
            VehicleId = item.VehicleId,
            VehicleCode = "",
            VehicleShortCode = "",
            CurrentHour = item.CurrentHour,
            CurrentKM = item.CurrentKM,
            Quantity = item.Quantity,
            Rate = item.Rate,
            Total = item.Total,
            Remarks = item.Remarks
        }));

        await SaveTransaction(transaction, itemIssueItemCarts);
    }

    public static async Task<int> SaveTransaction(ServiceModel service, List<ServiceItemCartModel> serviceDetails)
    {
        var update = service.Id > 0;

        if (update)
        {
            var existingService = await CommonData.LoadTableDataById<ServiceModel>(TableNames.Service, service.Id);
            var updateFinancialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, existingService.FinancialYearId);
            if (updateFinancialYear is null || updateFinancialYear.Locked || updateFinancialYear.Status == false)
                throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

            service.TransactionNo = existingService.TransactionNo;
        }
        else
            service.TransactionNo = await GenerateCodes.GenerateServiceTransactionNo(service);

        var financialYear = await CommonData.LoadTableDataById<FinancialYearModel>(TableNames.FinancialYear, service.FinancialYearId);
        if (financialYear is null || financialYear.Locked || financialYear.Status == false)
            throw new InvalidOperationException("Cannot update transaction as the financial year is locked.");

        service.Id = await InsertService(service);
        await SaveTransactionDetail(service, serviceDetails, update);

        return service.Id;
    }

    private static async Task SaveTransactionDetail(ServiceModel transaction,
        List<ServiceItemCartModel> transactionDetails, bool update)
    {
        if (update)
        {
            var existingDetails = await CommonData.LoadTableDataByMasterId<ServiceDetailModel>(TableNames.ServiceDetail, transaction.Id);
            foreach (var item in existingDetails)
            {
                item.Status = false;
                await InsertServiceDetail(item);
            }
        }

        foreach (var item in transactionDetails)
            await InsertServiceDetail(new()
            {
                Id = 0,
                MasterId = transaction.Id,
                ServiceTypeId = item.ServiceTypeId,
                VehicleId = item.VehicleId,
                CurrentHour = item.CurrentHour,
                CurrentKM = item.CurrentKM,
                Quantity = item.Quantity,
                Rate = item.Rate,
                Total = item.Total,
                Remarks = item.Remarks,
                Status = true
            });
    }
}
