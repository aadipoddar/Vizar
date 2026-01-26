CREATE VIEW [dbo].[PurchaseOrder_Item_Overview]
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

	[po].[Id] AS MasterId,
	[po].[TransactionNo],
	[po].[TransactionDateTime],
	[c].[Id] AS CompanyId,
	[c].[Name] AS CompanyName,
	[l].[Id] AS VendorId,
	[l].[Name] AS VendorName,
	[g].[Id] AS GarageId,
	[g].[Name] AS GarageName,

	[po].[PurchaseId],
	[p].[TransactionNo] AS PurchaseTransactionNo,
	[p].[TransactionDateTime] AS PurchaseDateTime,
	[p].[ReceiveDateTime] AS PurchaseReceiveDateTime,
	[pd].[Quantity] AS PurchaseQuantity,
	[po].[Remarks] AS PurchaseOrderRemarks,

	[pod].[UnitOfMeasurement],
	[pod].[Quantity],
	[pod].[Remarks]

FROM
	[dbo].[PurchaseOrderDetail] pod

INNER JOIN
	[dbo].[PurchaseOrder] po ON pod.[MasterId] = po.Id
INNER JOIN
	[dbo].[Item] i ON pod.ItemId = i.Id
INNER JOIN
	[dbo].[ItemCategory] ic ON i.ItemCategoryId = ic.Id
INNER JOIN
	[dbo].[ItemType] it ON i.ItemTypeId = it.Id
INNER JOIN
	[dbo].[Manufacturer] m ON i.ManufacturerId = m.Id
INNER JOIN
	[dbo].[Company] c ON po.CompanyId = c.Id
INNER JOIN
	[dbo].[Ledger] l ON po.[VendorId] = l.Id
INNER JOIN
	[dbo].[Garage] g ON po.[GarageId] = g.Id
LEFT JOIN
	[dbo].[Purchase] p ON po.PurchaseId = p.Id
LEFT JOIN
	[dbo].[PurchaseDetail] pd ON p.Id = pd.MasterId AND i.Id = pd.ItemId

WHERE
	[po].[Status] = 1 AND
	[pod].[Status] = 1;