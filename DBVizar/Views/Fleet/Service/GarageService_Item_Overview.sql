CREATE VIEW [dbo].[GarageService_Item_Overview]
	AS
SELECT
	[st].[Id] AS ServiceTypeId,
	[st].[Name] AS ServiceTypeName,
	[st].[Code] AS ServiceTypeCode,

	[s].[Id] AS MasterId,
	[s].[TransactionNo],
	[s].[TransactionDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[s].[GarageId],
	[g].[Name] AS GarageName,
	[v].[Id] AS VehicleId,
	[v].[Code] AS VehicleCode,
	[s].[Remarks] AS ServiceRemarks,

	[sd].[Quantity],
	[sd].[Rate],
	[sd].[Total],

	[sd].[Remarks]

FROM
	[dbo].[ServiceDetail] sd

INNER JOIN
	[dbo].[Service] s ON sd.[MasterId] = s.Id
INNER JOIN
	[dbo].[ServiceType] st ON sd.ServiceTypeId = st.Id
INNER JOIN
	[dbo].[Company] c ON s.CompanyId = c.Id
INNER JOIN
	[dbo].[Garage] g ON s.GarageId = g.Id
INNER JOIN
	[dbo].[Vehicle] v ON sd.VehicleId = v.Id

WHERE
	[s].[Status] = 1 AND
	[sd].[Status] = 1;