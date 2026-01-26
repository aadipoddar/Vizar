CREATE VIEW [dbo].[PurchaseOrder_Overview]
	AS
SELECT
	[po].[Id],
	[po].[TransactionNo],

	[c].[Name] AS CompanyName,
	[po].[VendorId],
	[l].[Name] AS VendorName,
	[po].[GarageId],
	[g].[Name] AS GarageName,

	[po].[PurchaseId],
	[p].[TransactionNo] AS PurchaseTransactionNo,
	[p].[TransactionDateTime] AS PurchaseDateTime,
	[p].[ReceiveDateTime] AS PurchaseReceiveDateTime,

	[po].[TransactionDateTime],
	[po].[FinancialYearId],
	CONVERT(VARCHAR(10), fy.StartDate, 103) + ' to ' + CONVERT(VARCHAR(10), fy.EndDate, 103) AS FinancialYear,

	[po].[TotalItems],
	[po].[TotalQuantity],

	[po].[Remarks],
	[po].[CreatedBy],
	[u].[Name] AS CreatedByName,
	[po].[CreatedAt],
	[po].[CreatedFromPlatform],
	[po].[LastModifiedBy],
	[lm].[Name] AS LastModifiedByUserName,
	[po].[LastModifiedAt],
	[po].[LastModifiedFromPlatform],
	[po].[Status]

FROM
	[dbo].[PurchaseOrder] po

INNER JOIN
	[dbo].[Company] AS c ON po.CompanyId = c.Id
INNER JOIN
	[dbo].[Ledger] AS l ON po.[VendorId] = l.Id
INNER JOIN
	[dbo].[Garage] AS g ON po.GarageId = g.Id
LEFT JOIN
	[dbo].[Purchase] p ON po.PurchaseId = p.Id AND p.Status = 1
INNER JOIN
	[dbo].[FinancialYear] AS fy ON po.FinancialYearId = fy.Id
INNER JOIN
	[dbo].[User] AS u ON po.CreatedBy = u.Id
LEFT JOIN
	[dbo].[User] AS lm ON po.LastModifiedBy = lm.Id