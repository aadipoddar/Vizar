CREATE VIEW [dbo].[Service_Overview]
	AS
	SELECT
		[s].[Id],
		[s].[TransactionNo],
		[s].[CompanyId],
		[c].[Name] AS CompanyName,
		[s].[TransactionDateTime],
		[s].[FinancialYearId],
		CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

		[s].[GarageId],
		[g].[Name] AS GarageName,
		
		[s].[TotalItems],
		[s].[TotalQuantity],
		[s].[TotalAmount],
		
		[s].[Remarks],
		[s].[CreatedBy],
		[u].[Name] AS CreatedByName,
		[s].[CreatedAt],
		[s].[CreatedFromPlatform],
		[s].[LastModifiedBy],
		[lm].[Name] AS LastModifiedByUserName,
		[s].[LastModifiedAt],
		[s].[LastModifiedFromPlatform],

		[s].[Status]

	FROM
		[dbo].[Service] s
	INNER JOIN
		[dbo].[Company] AS c ON s.CompanyId = c.Id
	INNER JOIN
		[dbo].[FinancialYear] AS fy ON s.FinancialYearId = fy.Id
	LEFT JOIN
		[dbo].[Garage] AS g ON s.GarageId = g.Id
	INNER JOIN
		[dbo].[User] AS u ON s.[CreatedBy] = u.Id
	LEFT JOIN
		[dbo].[User] AS lm ON s.LastModifiedBy = lm.Id