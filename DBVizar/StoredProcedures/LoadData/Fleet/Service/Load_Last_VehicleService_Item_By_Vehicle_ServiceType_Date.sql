CREATE PROCEDURE [dbo].[Load_Last_VehicleService_Item_By_Vehicle_ServiceType_Date]
	@VehicleId INT,
	@ServiceTypeId INT,
	@TransactionDateTime DATETIME
AS
BEGIN
	SELECT TOP 1 *
	FROM [dbo].[VehicleService_Item_Overview]
	WHERE [VehicleId] = @VehicleId
		AND [ServiceTypeId] = @ServiceTypeId
		AND [TransactionDateTime] <= @TransactionDateTime
	ORDER BY [TransactionDateTime] DESC
END