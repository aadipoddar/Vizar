CREATE VIEW [dbo].[ItemStockDetails]
	AS
SELECT
	ITST.Id,
	ITST.ItemId,
	IT.Name AS ItemName,
	IT.Code AS ItemCode,
	ITT.Id AS ItemTypeId,
	ITT.Name AS ItemTypeName,
	ITC.Id AS ItemCategoryId,
	ITC.Name AS ItemCategoryName,
	MFR.Id AS ManufacturerId,
	MFR.Name AS ManufacturerName,
	ITST.IdentificationNo,
	ITST.GarageId,
	GRG.Name AS GarageName,
	ITST.Quantity,
	ITST.NetRate,
	ITST.Type,
	ITST.TransactionId,
	ITST.TransactionNo,
	ITST.TransactionDateTime

FROM
	ItemStock AS ITST

INNER JOIN
	Item AS IT ON ITST.ItemId = IT.Id
INNER JOIN
	ItemCategory AS ITC ON IT.ItemCategoryId = ITC.Id
INNER JOIN
	ItemType AS ITT ON IT.ItemTypeId = ITT.Id
INNER JOIN
	Manufacturer AS MFR ON IT.ManufacturerId = MFR.Id
INNER JOIN
	Garage AS GRG ON ITST.GarageId = GRG.Id;