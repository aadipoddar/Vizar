CREATE VIEW [dbo].[OutsideRepair_Overview]
	AS
	SELECT
		[outr].[Id],
		[outr].[TransactionNo],
		[outr].[CompanyId],
		[c].[Name] AS CompanyName,
		[outr].[TransactionDateTime],
		[outr].[FinancialYearId],
		CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

		[outr].[VendorId],
		[g].[Name] AS VendorName,

		[outr].[VehicleId],
		[v].[Code] AS VehicleCode,
		[outr].[CurrentHour],
		[outr].[CurrentKM],
		[outr].[ApprovedBy],
		
		[outr].[TotalItems],
		[outr].[TotalQuantity],
		[outr].[TotalAmount],
		
		[outr].[Remarks],
		[outr].[CreatedBy],
		[u].[Name] AS CreatedByName,
		[outr].[CreatedAt],
		[outr].[CreatedFromPlatform],
		[outr].[LastModifiedBy],
		[lm].[Name] AS LastModifiedByUserName,
		[outr].[LastModifiedAt],
		[outr].[LastModifiedFromPlatform],

		[outr].[Status]

	FROM
		dbo.OutsideRepair outr
	INNER JOIN
		[dbo].[Company] AS c ON outr.CompanyId = c.Id
	INNER JOIN
		[dbo].[FinancialYear] AS fy ON outr.FinancialYearId = fy.Id
	INNER JOIN
		[dbo].[Ledger] AS g ON outr.VendorId = g.Id
	INNER JOIN
		[dbo].[Vehicle] AS v ON outr.VehicleId = v.Id
	INNER JOIN
		[dbo].[User] AS u ON outr.[CreatedBy] = u.Id
	LEFT JOIN
		[dbo].[User] AS lm ON outr.LastModifiedBy = lm.Id