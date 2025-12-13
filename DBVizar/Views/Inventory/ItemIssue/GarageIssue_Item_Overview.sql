CREATE VIEW [dbo].[GarageIssue_Item_Overview]
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

	[iid].[IdentificationNo],
	[iid].[UnitOfMeasurement],
	[iid].[Quantity],
	[iid].[Rate],
	[iid].[Total],

	[iid].[Remarks] AS Remarks

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
INNER JOIN
	[dbo].[Company] c ON ii.CompanyId = c.Id
LEFT JOIN
	[dbo].[Garage] g ON ii.GarageId = g.Id

WHERE
	[ii].[GarageId] IS NOT NULL AND
	[ii].[Status] = 1 AND
	[iid].[Status] = 1;