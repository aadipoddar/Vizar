CREATE VIEW [dbo].[Document_Overview]
	AS
	SELECT
		[d].[Id],
		[d].[TransactionNo],
		[d].[TransactionDateTime],
		[d].[FinancialYearId],
		CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

		[d].[DocumentTypeId],
		[dt].[Name] AS DocumentType,
		[d].[VehicleId],
		[v].[Code] AS Vehicle,
		[d].[CurrentHour],
		[d].[CurrentKM],
		[d].[Rate],
		[d].[RenewalDate],

		[d].[Remarks],
		[d].[DocumentUrl],
		[d].[CreatedBy],
		[u].[Name] AS CreatedByName,
		[d].[CreatedAt],
		[d].[CreatedFromPlatform],
		[d].[LastModifiedBy],
		[lm].[Name] AS LastModifiedByUserName,
		[d].[LastModifiedAt],
		[d].[LastModifiedFromPlatform],

		[d].[Status]

	FROM
		[dbo].[Document] d
	INNER JOIN
		[dbo].[FinancialYear] AS fy ON d.FinancialYearId = fy.Id
	INNER JOIN
		[dbo].[DocumentType] AS dt ON d.DocumentTypeId = dt.Id
	INNER JOIN
		[dbo].[Vehicle] AS v ON d.VehicleId = v.Id
	INNER JOIN
		[dbo].[User] AS u ON d.[CreatedBy] = u.Id
	LEFT JOIN
		[dbo].[User] AS lm ON d.LastModifiedBy = lm.Id