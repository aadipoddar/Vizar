CREATE PROCEDURE [dbo].[Load_Item_By_Party_PurchaseDateTime]
	@PartyId INT,
	@PurchaseDateTime DATETIME,
	@OnlyActive BIT
AS
BEGIN
	SELECT
		i.[Id],
		i.[Name],
		i.[Code],
		i.[ItemType],
		i.[ItemCategory],
		i.[ManufacturerId],

		ISNULL(
			CASE 
				WHEN @PartyId > 0 THEN
					(SELECT TOP 1 Rate FROM PurchaseDetail pd
					 INNER JOIN Purchase p ON pd.PurchaseId = p.Id
					 WHERE pd.ItemId = i.[Id]
					   AND p.PartyId = @PartyId
					   AND p.Status = 1
					   AND pd.Status = 1
					   AND p.TransactionDateTime <= @PurchaseDateTime
					 ORDER BY pd.Id DESC)
				ELSE
					(SELECT TOP 1 Rate FROM PurchaseDetail pd
					 INNER JOIN Purchase p ON pd.PurchaseId = p.Id
					 WHERE pd.ItemId = i.[Id]
					   AND p.Status = 1
					   AND pd.Status = 1
					   AND p.TransactionDateTime <= @PurchaseDateTime
					 ORDER BY pd.Id DESC)
			END, i.[Rate]) AS [Rate],

		ISNULL(
			CASE 
				WHEN @PartyId > 0 THEN
					(SELECT TOP 1 TaxId FROM PurchaseDetail pd
					 INNER JOIN Purchase p ON pd.PurchaseId = p.Id
					 WHERE pd.ItemId = i.[Id]
					   AND p.PartyId = @PartyId
					   AND p.Status = 1
					   AND pd.Status = 1
					   AND p.TransactionDateTime <= @PurchaseDateTime
					   ORDER BY pd.Id DESC)
				ELSE
					(SELECT TOP 1 TaxId FROM PurchaseDetail pd
					 INNER JOIN Purchase p ON pd.PurchaseId = p.Id
					 WHERE pd.ItemId = i.[Id]
					   AND p.Status = 1
					   AND pd.Status = 1
					   AND p.TransactionDateTime <= @PurchaseDateTime
					   ORDER BY pd.Id DESC)
			END, i.[TaxId]) AS [TaxId],


		ISNULL(
			CASE 
				WHEN @PartyId > 0 THEN
					(SELECT TOP 1 UnitOfMeasurement FROM PurchaseDetail pd
					 INNER JOIN Purchase p ON pd.PurchaseId = p.Id
					 WHERE pd.ItemId = i.[Id]
					   AND p.PartyId = @PartyId
					   AND p.Status = 1
					   AND pd.Status = 1
					   AND p.TransactionDateTime <= @PurchaseDateTime
					   ORDER BY pd.Id DESC)
				ELSE
					(SELECT TOP 1 UnitOfMeasurement FROM PurchaseDetail pd
					 INNER JOIN Purchase p ON pd.PurchaseId = p.Id
					 WHERE pd.ItemId = i.[Id]
					   AND p.Status = 1
					   AND pd.Status = 1
					   AND p.TransactionDateTime <= @PurchaseDateTime
					   ORDER BY pd.Id DESC)
		END, i.[UnitOfMeasurement]) AS [UnitOfMeasurement],

		i.[ReorderLevel],
		i.[Remarks],
		i.[Status]

	FROM Item i
	WHERE (@OnlyActive = 0 OR i.[Status] = 1)
END