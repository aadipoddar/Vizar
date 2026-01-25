CREATE VIEW [dbo].[InsideRepair_Overview]
	AS
	SELECT
		[ir].[Id],
		[ir].[TransactionNo],
		[ir].[CompanyId],
		[c].[Name] AS CompanyName,
		[ir].[TransactionDateTime],
		[ir].[FinancialYearId],
		CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

		[ir].[GarageId],
		[g].[Name] AS GarageName,

		[ir].[VehicleId],
		[v].[Code] AS VehicleCode,
		[ir].[CurrentHour],
		[ir].[CurrentKM],
		
		[ir].[TotalItems],
		[ir].[TotalQuantity],
		[ir].[TotalAmount],
		
		[ir].[Remarks],
		[ir].[CreatedBy],
		[u].[Name] AS CreatedByName,
		[ir].[CreatedAt],
		[ir].[CreatedFromPlatform],
		[ir].[LastModifiedBy],
		[lm].[Name] AS LastModifiedByUserName,
		[ir].[LastModifiedAt],
		[ir].[LastModifiedFromPlatform],

		[ir].[Status]

	FROM
		dbo.InsideRepair ir
	INNER JOIN
		[dbo].[Company] AS c ON ir.CompanyId = c.Id
	INNER JOIN
		[dbo].[FinancialYear] AS fy ON ir.FinancialYearId = fy.Id
	INNER JOIN
		[dbo].[Garage] AS g ON ir.GarageId = g.Id
	INNER JOIN
		[dbo].[Vehicle] AS v ON ir.VehicleId = v.Id
	INNER JOIN
		[dbo].[User] AS u ON ir.[CreatedBy] = u.Id
	LEFT JOIN
		[dbo].[User] AS lm ON ir.LastModifiedBy = lm.Id