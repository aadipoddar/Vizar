CREATE VIEW [dbo].[PurchaseReturn_Item_Overview]
	AS
SELECT
	[i].[Id],
	[i].[Name] AS ItemName,
	[i].[Code] AS ItemCode,
	[ic].[Id] AS ItemCategoryId,
	[ic].[Name] AS ItemCategoryName,
	[it].[Id] AS ItemTypeId,
	[it].[Name] AS ItemTypeName,
	[m].[Id] AS ManufacturerId,
	[m].[Name] AS ManufacturerName,

	[p].[Id] AS PurchaseReturnId,
	[p].[TransactionNo],
	[p].[TransactionDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[l].[Id] AS PartyId,
	[l].[Name] AS PartyName,
	[p].[Remarks] AS PurchaseReturnRemarks,

	[pd].[IdentificationNo] AS IdentificationNo,

	[pd].[Quantity],
	[pd].[Rate],
	[pd].[BaseTotal],

	[pd].[DiscountPercent],
	[pd].[DiscountAmount],
	[pd].[AfterDiscount],

	[pd].[CGSTPercent],
	[pd].[CGSTAmount],
	[pd].[SGSTPercent],
	[pd].[SGSTAmount],
	[pd].[IGSTPercent],
	[pd].[IGSTAmount],
	[pd].[TotalTaxAmount],
	[pd].[InclusiveTax],

	[pd].[Total],
	[pd].[NetRate],

	[pd].[Remarks] AS Remarks

FROM
	[dbo].[PurchaseReturnDetail] pd

INNER JOIN
	[dbo].[PurchaseReturn] p ON pd.PurchaseReturnId = p.Id
INNER JOIN
	[dbo].[Item] i ON pd.ItemId = i.Id
INNER JOIN
	[dbo].[ItemCategory] ic ON i.ItemCategoryId = ic.Id
INNER JOIN
	[dbo].[ItemType] it ON i.ItemTypeId = it.Id
INNER JOIN
	[dbo].[Manufacturer] m ON i.ManufacturerId = m.Id
INNER JOIN
	[dbo].[Company] c ON p.CompanyId = c.Id
INNER JOIN
	[dbo].[Ledger] l ON p.PartyId = l.Id

WHERE
	[p].[Status] = 1 AND
	[pd].[Status] = 1;