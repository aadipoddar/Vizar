CREATE VIEW [dbo].[ItemIssue_Item_Overview]
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
	[ii].[GarageId],
	[g].[Name] AS GarageName,
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

	-- Mileage of the vehicle at the time of item issue can be calculated as CurrentKM - PreviousKM + CurrentHour - PreviousHour / Quantity where VehicleId can be null KM and Hr can be null as well
	[iid].[CurrentHour] AS PreviousHour,
	[iid].[CurrentKM] AS PreviousKM,
	0 AS Average

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
LEFT JOIN
	[dbo].[Garage] g ON ii.GarageId = g.Id

WHERE
	[ii].[Status] = 1 AND
	[iid].[Status] = 1;