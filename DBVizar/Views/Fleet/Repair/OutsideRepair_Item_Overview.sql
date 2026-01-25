CREATE VIEW [dbo].[OutsideRepair_Item_Overview]
	AS
SELECT
	[ord].[Job],

	[outr].[Id] AS MasterId,
	[outr].[TransactionNo],
	[outr].[TransactionDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[outr].[VendorId],
	[g].[Name] AS VendorName,
	[outr].[VehicleId],
	[v].[Code] AS VehicleCode,
	[outr].[CurrentHour],
	[outr].[CurrentKM],
	[outr].[ApprovedBy],
	[outr].[Remarks] AS OutsideRepairRemarks,
	
	[ord].[Quantity],
	[ord].[Rate],
	[ord].[Total],

	[ord].[Remarks]

FROM
	[dbo].[OutsideRepairDetail] ord

INNER JOIN
	[dbo].[OutsideRepair] outr ON ord.[MasterId] = outr.Id
INNER JOIN
	[dbo].[Company] c ON outr.CompanyId = c.Id
INNER JOIN
	[dbo].[Ledger] g ON outr.VendorId = g.Id
INNER JOIN
	[dbo].[Vehicle] v ON outr.VehicleId = v.Id

WHERE
	[outr].[Status] = 1 AND
	[ord].[Status] = 1;