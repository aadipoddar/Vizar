CREATE VIEW [dbo].[ItemIssue_Overview]
	AS
	SELECT
		[ii].[Id],
		[ii].[TransactionNo],
		[ii].[CompanyId],
		[c].[Name] AS CompanyName,
		[ii].[TransactionDateTime],
		[ii].[FinancialYearId],
		CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

		[ii].[GarageId],
		[g].[Name] AS GarageName,
		
		[ii].[TotalItems],
		[ii].[TotalQuantity],
		[ii].[TotalAmount],
		
		[ii].[Remarks],
		[ii].[CreatedBy],
		[u].[Name] AS CreatedByName,
		[ii].[CreatedAt],
		[ii].[CreatedFromPlatform],
		[ii].[LastModifiedBy],
		[lm].[Name] AS LastModifiedByUserName,
		[ii].[LastModifiedAt],
		[ii].[LastModifiedFromPlatform],

		[ii].[Status]

	FROM
		dbo.ItemIssue ii
	INNER JOIN
		[dbo].[Company] AS c ON ii.CompanyId = c.Id
	INNER JOIN
		[dbo].[FinancialYear] AS fy ON ii.FinancialYearId = fy.Id
	LEFT JOIN
		[dbo].[Garage] AS g ON ii.GarageId = g.Id
	INNER JOIN
		[dbo].[User] AS u ON ii.[CreatedBy] = u.Id
	LEFT JOIN
		[dbo].[User] AS lm ON ii.LastModifiedBy = lm.Id