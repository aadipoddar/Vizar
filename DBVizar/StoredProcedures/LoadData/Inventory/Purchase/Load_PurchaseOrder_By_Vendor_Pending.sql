CREATE PROCEDURE [dbo].[Load_PurchaseOrder_By_Garage_Vendor_Pending]
	@GarageId INT,
	@VendorId INT
AS
BEGIN
	SELECT
		*
	FROM [dbo].[PurchaseOrder] po
	WHERE po.VendorId = @VendorId
		AND po.GarageId = @GarageId
		AND po.PurchaseId IS NULL
		AND po.Status = 1
END