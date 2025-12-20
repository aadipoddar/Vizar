CREATE VIEW [dbo].[VehicleService_Item_Overview]
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
	[s].[Remarks] AS ServiceRemarks,

	[sd].[VehicleId],
	[v].[Code] AS VehicleCode,
	[v].[ShortCode] AS VehicleShortCode,
	[sd].[CurrentHour],
	[sd].[CurrentKM],
	[sd].[Quantity],
	[sd].[Rate],
	[sd].[Total],

	[sd].[Remarks],

	-- Previous values from OUTER APPLY
	[prev].[CurrentHour] AS PreviousHour,
	[prev].[CurrentKM] AS PreviousKM,
	
	-- Calculate average based on distance traveled
	CASE
		WHEN [sd].[Quantity] > 0 AND (
			(ISNULL([sd].[CurrentKM], 0) - ISNULL([prev].[CurrentKM], 0)) +
			(ISNULL([sd].[CurrentHour], 0) - ISNULL([prev].[CurrentHour], 0))
		) > 0
		THEN  ((ISNULL([sd].[CurrentKM], 0) - ISNULL([prev].[CurrentKM], 0)) +
				(ISNULL([sd].[CurrentHour], 0) - ISNULL([prev].[CurrentHour], 0))) / [sd].[Quantity]
		ELSE NULL
	END AS Average,

	[ss].[IntervalDays],
	-- Calculate next due date by adding interval days to transaction date time
	CASE
		WHEN [ss].[IntervalDays] IS NOT NULL AND [ss].[IntervalDays] > 0
			THEN DATEADD(DAY, [ss].[IntervalDays], [s].[TransactionDateTime])
		ELSE NULL
	END AS NextDueDate

FROM
	[dbo].[ServiceDetail] sd

INNER JOIN
	[dbo].[Service] s ON sd.[MasterId] = s.Id
INNER JOIN
	[dbo].[ServiceType] st ON sd.ServiceTypeId = st.Id
INNER JOIN
	[dbo].[Garage] g ON s.GarageId = g.Id
INNER JOIN
	[dbo].[Vehicle] v ON sd.VehicleId = v.Id
INNER JOIN
	[dbo].[Company] c ON s.CompanyId = c.Id
RIGHT JOIN
	[dbo].[ServiceSchedule] ss ON sd.ServiceTypeId = ss.ServiceTypeId AND v.VehicleTypeId = ss.VehicleTypeId AND ss.[Status] = 1

-- Get previous readings for the same vehicle and item
OUTER APPLY (
	SELECT TOP 1 
		prevDetail.[CurrentHour],
		prevDetail.[CurrentKM]
	FROM [dbo].[ServiceDetail] prevDetail
	INNER JOIN [dbo].[Service] prevMaster ON prevDetail.[MasterId] = prevMaster.Id
	WHERE prevDetail.[VehicleId] = [sd].[VehicleId]
		AND prevDetail.[ServiceTypeId] = [sd].[ServiceTypeId]
		AND prevMaster.[TransactionDateTime] < [s].[TransactionDateTime]
		AND prevMaster.[Status] = 1
		AND prevDetail.[Status] = 1
	ORDER BY prevMaster.[TransactionDateTime] DESC, prevDetail.[Id] DESC
) prev

WHERE
	[s].[Status] = 1 AND
	[sd].[Status] = 1;