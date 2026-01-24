CREATE VIEW [dbo].[Purchase_Item_Overview]
	AS
SELECT
	[i].[Id],
	[i].[Name] AS ItemName,
	[i].[Code] AS ItemCode,
	[it].[Id] AS ItemTypeId,
	[it].[Name] AS ItemTypeName,
	[ic].[Id] AS ItemCategoryId,
	[ic].[Name] AS ItemCategoryName,
	[m].[Id] AS ManufacturerId,
	[m].[Name] AS ManufacturerName,

	[p].[Id] AS MasterId,
	[p].[TransactionNo],
	[p].[TransactionDateTime],
	[p].[ReceiveDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[l].[Id] AS VendorId,
	[l].[Name] AS VendorName,
	[g].[Id] AS GarageId,
	[g].[Name] AS GarageName,
	[p].[Remarks] AS PurchaseRemarks,

	[pd].[IdentificationNo],
	[pd].[UnitOfMeasurement],
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
	[pd].[NetRate] * [pd].[Quantity] AS NetTotal,

	[pd].[Remarks] AS Remarks

FROM
	[dbo].[PurchaseDetail] pd

INNER JOIN
	[dbo].[Purchase] p ON pd.[MasterId] = p.Id
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
	[dbo].[Ledger] l ON p.[VendorId] = l.Id
INNER JOIN
	[dbo].[Garage] g ON p.[GarageId] = g.Id

WHERE
	[p].[Status] = 1 AND
	[pd].[Status] = 1;