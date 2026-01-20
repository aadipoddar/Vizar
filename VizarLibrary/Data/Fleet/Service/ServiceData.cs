using VizarLibrary.Data.Accounts.Masters;
using VizarLibrary.Data.Common;
using VizarLibrary.DataAccess;
using VizarLibrary.Exporting.Fleet.Service;
using VizarLibrary.Exporting.Utils;
using VizarLibrary.Models.Fleet.Service;

namespace VizarLibrary.Data.Fleet.Service;

public static class ServiceData
{
    public static async Task<int> InsertServiceType(ServiceTypeModel serviceType) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertServiceType, serviceType)).FirstOrDefault();

    private static async Task<int> InsertService(ServiceModel service, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertService, service, sqlDataAccessTransaction)).FirstOrDefault();

    private static async Task<int> InsertServiceDetail(ServiceDetailModel serviceDetail, SqlDataAccessTransaction sqlDataAccessTransaction = null) =>
        (await SqlDataAccess.LoadData<int, dynamic>(StoredProcedureNames.InsertServiceDetail, serviceDetail, sqlDataAccessTransaction)).FirstOrDefault();

    public static async Task<VehicleServiceItemOverviewModel> LoadLastVehicleServiceItemByVehicleServiceTypeDate(int VehicleId, int ServiceTypeId, DateTime TransactionDateTime) =>
        (await SqlDataAccess.LoadData<VehicleServiceItemOverviewModel, dynamic>(StoredProcedureNames.LoadLastVehicleServiceItemByVehicleServiceTypeDate, new { VehicleId, ServiceTypeId, TransactionDateTime })).FirstOrDefault();

    public static List<ServiceDetailModel> ConvertCartToDetails(List<ServiceItemCartModel> cart, int serviceId) =>
        [.. cart.Select(item => new ServiceDetailModel
        {
            Id = 0,
            MasterId = serviceId,
            ServiceTypeId = item.ServiceTypeId,
            VehicleId = item.VehicleId,
            CurrentHour = item.CurrentHour,
            CurrentKM = item.CurrentKM,
            Quantity = item.Quantity,
            Rate = item.Rate,
            Total = item.Total,
            Remarks = item.Remarks,
            Status = true
        })];

    public static async Task DeleteTransaction(ServiceModel service)
    {
        using SqlDataAccessTransaction sqlDataAccessTransaction = new();

        try
        {
            sqlDataAccessTransaction.StartTransaction();

            await FinancialYearData.ValidateFinancialYear(service.TransactionDateTime, sqlDataAccessTransaction);

            service.Status = false;
            await InsertService(service, sqlDataAccessTransaction);

            sqlDataAccessTransaction.CommitTransaction();

            await ServiceNotify.Notify(service.Id, NotifyType.Deleted);
        }
        catch
        {
            sqlDataAccessTransaction.RollbackTransaction();
            throw;
        }
    }

    public static async Task RecoverTransaction(ServiceModel service)
    {
        service.Status = true;
        var serviceDetails = await CommonData.LoadTableDataByMasterId<ServiceDetailModel>(TableNames.ServiceDetail, service.Id);

        await SaveTransaction(service, null, serviceDetails);

        await ServiceNotify.Notify(service.Id, NotifyType.Recovered);
    }

    public static async Task<int> SaveTransaction(ServiceModel service, List<ServiceItemCartModel> cart, List<ServiceDetailModel> serviceDetails = null, bool showNotification = true, SqlDataAccessTransaction sqlDataAccessTransaction = null)
    {
        bool update = service.Id > 0;

        if (sqlDataAccessTransaction is null)
        {
            (MemoryStream, string)? previousInvoice = null;
            if (update)
                previousInvoice = await ServiceInvoiceExport.ExportInvoice(service.Id, InvoiceExportType.PDF);

            using SqlDataAccessTransaction newSqlDataAccessTransaction = new();

            try
            {
                newSqlDataAccessTransaction.StartTransaction();
                service.Id = await SaveTransaction(service, cart, serviceDetails, showNotification, newSqlDataAccessTransaction);
                newSqlDataAccessTransaction.CommitTransaction();
            }
            catch
            {
                newSqlDataAccessTransaction.RollbackTransaction();
                throw;
            }

            if (showNotification)
                await ServiceNotify.Notify(service.Id, update ? NotifyType.Updated : NotifyType.Created, previousInvoice);

            return service.Id;
        }

        if (update)
        {
            var existingService = await CommonData.LoadTableDataById<ServiceModel>(TableNames.Service, service.Id, sqlDataAccessTransaction);
            await FinancialYearData.ValidateFinancialYear(existingService.TransactionDateTime, sqlDataAccessTransaction);
        }

        await FinancialYearData.ValidateFinancialYear(service.TransactionDateTime, sqlDataAccessTransaction);

        service.Id = await InsertService(service, sqlDataAccessTransaction);
        serviceDetails ??= ConvertCartToDetails(cart, service.Id);
        await SaveTransactionDetail(service, serviceDetails, update, sqlDataAccessTransaction);

        return service.Id;
    }

    private static async Task SaveTransactionDetail(ServiceModel service, List<ServiceDetailModel> serviceDetails, bool update, SqlDataAccessTransaction sqlDataAccessTransaction)
    {
        if (serviceDetails is null || serviceDetails.Count != service.TotalItems || serviceDetails.Sum(d => d.Quantity) != service.TotalQuantity)
            throw new InvalidOperationException("Service details do not match the transaction summary.");

        if (serviceDetails.Any(d => !d.Status))
            throw new InvalidOperationException("Service detail items must be active.");

        if (update)
        {
            var existingServiceDetails = await CommonData.LoadTableDataByMasterId<ServiceDetailModel>(TableNames.ServiceDetail, service.Id, sqlDataAccessTransaction);
            foreach (var item in existingServiceDetails)
            {
                item.Status = false;
                await InsertServiceDetail(item, sqlDataAccessTransaction);
            }
        }

        foreach (var item in serviceDetails)
        {
            item.MasterId = service.Id;
            var id = await InsertServiceDetail(item, sqlDataAccessTransaction);

            if (id <= 0)
                throw new InvalidOperationException("Failed to save service detail item.");
        }
    }
}
