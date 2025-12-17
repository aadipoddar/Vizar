CREATE VIEW [dbo].[VehicleIssue_Item_Overview]
	AS
SELECT
	[i].[Id],
	[i].[Name] AS ItemName,
	[i].[Code] AS ItemCode,
	[it].[Id] AS ItemTypeId,
	[it].[Name] AS ItemTypeName,
	[ic].[Id] AS ItemCategoryId,
	[ic].[Name] AS ItemCategoryName,
	[m].[Id] AS ManufacturerId,
	[m].[Name] AS ManufacturerName,

	[ii].[Id] AS MasterId,
	[ii].[TransactionNo],
	[ii].[TransactionDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[ii].[Remarks] AS ItemIssueRemarks,

	[iid].[VehicleId],
	[v].[Code] AS VehicleCode,
	[v].[ShortCode] AS VehicleShortCode,
	[iid].[CurrentHour],
	[iid].[CurrentKM],
	[iid].[IdentificationNo],
	[iid].[UnitOfMeasurement],
	[iid].[Quantity],
	[iid].[Rate],
	[iid].[Total],

	[iid].[Remarks] AS Remarks,

	-- Previous values from OUTER APPLY
	[prev].[CurrentHour] AS PreviousHour,
	[prev].[CurrentKM] AS PreviousKM,
	
	-- Calculate average based on distance traveled
	CASE
		WHEN [iid].[Quantity] > 0 AND (
			(ISNULL([iid].[CurrentKM], 0) - ISNULL([prev].[CurrentKM], 0)) +
			(ISNULL([iid].[CurrentHour], 0) - ISNULL([prev].[CurrentHour], 0))
		) > 0
		THEN [iid].[Quantity] / (
			(ISNULL([iid].[CurrentKM], 0) - ISNULL([prev].[CurrentKM], 0)) +
			(ISNULL([iid].[CurrentHour], 0) - ISNULL([prev].[CurrentHour], 0))
		)
		ELSE NULL
	END AS Average

FROM
	[dbo].[ItemIssueDetail] iid

INNER JOIN
	[dbo].[ItemIssue] ii ON iid.[MasterId] = ii.Id
INNER JOIN
	[dbo].[Item] i ON iid.ItemId = i.Id
INNER JOIN
	[dbo].[ItemCategory] ic ON i.ItemCategoryId = ic.Id
INNER JOIN
	[dbo].[ItemType] it ON i.ItemTypeId = it.Id
INNER JOIN
	[dbo].[Manufacturer] m ON i.ManufacturerId = m.Id
LEFT JOIN
	[dbo].[Vehicle] v ON iid.VehicleId = v.Id
INNER JOIN
	[dbo].[Company] c ON ii.CompanyId = c.Id

-- Get previous readings for the same vehicle and item
OUTER APPLY (
	SELECT TOP 1 
		prevDetail.[CurrentHour],
		prevDetail.[CurrentKM]
	FROM [dbo].[ItemIssueDetail] prevDetail
	INNER JOIN [dbo].[ItemIssue] prevMaster ON prevDetail.[MasterId] = prevMaster.Id
	WHERE prevDetail.[VehicleId] = [iid].[VehicleId]
		AND prevDetail.[ItemId] = [iid].[ItemId]
		AND prevDetail.[VehicleId] IS NOT NULL
		AND prevMaster.[TransactionDateTime] < [ii].[TransactionDateTime]
		AND prevMaster.[Status] = 1
		AND prevDetail.[Status] = 1
		AND prevMaster.[GarageId] IS NULL
	ORDER BY prevMaster.[TransactionDateTime] DESC, prevDetail.[Id] DESC
) prev

WHERE
	[ii].[GarageId] IS NULL AND
	[ii].[Status] = 1 AND
	[iid].[Status] = 1;