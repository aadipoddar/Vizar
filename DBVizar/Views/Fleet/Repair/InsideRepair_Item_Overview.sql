CREATE VIEW [dbo].[InsideRepair_Item_Overview]
	AS
SELECT
	[i].[Id] AS ItemId,
	[i].[Name] AS ItemName,
	[i].[Code] AS ItemCode,
	[it].[Id] AS ItemTypeId,
	[it].[Name] AS ItemTypeName,
	[ic].[Id] AS ItemCategoryId,
	[ic].[Name] AS ItemCategoryName,
	[m].[Id] AS ManufacturerId,
	[m].[Name] AS ManufacturerName,

	[ir].[Id] AS MasterId,
	[ir].[TransactionNo],
	[ir].[TransactionDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[ir].[GarageId],
	[g].[Name] AS GarageName,
	[ir].[VehicleId],
	[v].[Code] AS VehicleCode,
	[ir].[Remarks] AS InsideRepairRemarks,

	[ird].[IdentificationNo],
	[ird].[UnitOfMeasurement],
	[ird].[Quantity],
	[ird].[Rate],
	[ird].[Total],

	[ird].[Remarks],

	[ir].[CurrentHour],
	[ir].[CurrentKM],

	-- Previous values from OUTER APPLY
	[prev].[CurrentHour] AS PreviousHour,
	[prev].[CurrentKM] AS PreviousKM,
	
	-- Calculate average based on distance traveled
	CASE
		WHEN [ird].[Quantity] > 0 AND (
			(ISNULL([ir].[CurrentKM], 0) - ISNULL([prev].[CurrentKM], 0)) +
			(ISNULL([ir].[CurrentHour], 0) - ISNULL([prev].[CurrentHour], 0))
		) > 0
		THEN  ((ISNULL([ir].[CurrentKM], 0) - ISNULL([prev].[CurrentKM], 0)) +
				(ISNULL([ir].[CurrentHour], 0) - ISNULL([prev].[CurrentHour], 0))) / [ird].[Quantity]
		ELSE NULL
	END AS Average

FROM
	[dbo].[InsideRepairDetail] ird

INNER JOIN
	[dbo].[InsideRepair] ir ON ird.[MasterId] = ir.Id
INNER JOIN
	[dbo].[Item] i ON ird.ItemId = i.Id
INNER JOIN
	[dbo].[ItemCategory] ic ON i.ItemCategoryId = ic.Id
INNER JOIN
	[dbo].[ItemType] it ON i.ItemTypeId = it.Id
INNER JOIN
	[dbo].[Manufacturer] m ON i.ManufacturerId = m.Id
INNER JOIN
	[dbo].[Company] c ON ir.CompanyId = c.Id
INNER JOIN
	[dbo].[Garage] g ON ir.GarageId = g.Id
INNER JOIN
	[dbo].[Vehicle] v ON ir.VehicleId = v.Id

-- Get previous readings for the same vehicle
OUTER APPLY (
	SELECT TOP 1 
		prevMaster.[CurrentHour],
		prevMaster.[CurrentKM]
	FROM [dbo].[InsideRepairDetail] prevDetail
	INNER JOIN [dbo].[InsideRepair] prevMaster ON prevDetail.[MasterId] = prevMaster.Id
	WHERE prevMaster.[VehicleId] = [ir].[VehicleId]
		AND prevDetail.[ItemId] = [ird].[ItemId]
		AND prevMaster.[TransactionDateTime] < [ir].[TransactionDateTime]
		AND prevMaster.[Status] = 1
	ORDER BY prevMaster.[TransactionDateTime] DESC, prevMaster.[Id] DESC
) prev

WHERE
	[ir].[Status] = 1 AND
	[ird].[Status] = 1;