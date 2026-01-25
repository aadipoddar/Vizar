CREATE PROCEDURE [dbo].[Load_ItemStockSummary_By_Garage_Date]
	@GarageId INT,
	@FromDate DATETIME,
	@ToDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	-- Pre-calculate all stock aggregations in a single pass for each raw material
	WITH StockAggregates AS (
		SELECT 
			ItemId,
			-- Opening Stock: sum of all quantities before StartDate
			SUM(CASE WHEN TransactionDateTime < @FromDate THEN Quantity ELSE 0 END) AS OpeningStock,
			
			-- Purchase Stock: sum of positive quantities in date range
			SUM(CASE WHEN TransactionDateTime >= @FromDate AND TransactionDateTime <= @ToDate AND Quantity > 0 
				THEN Quantity ELSE 0 END) AS PurchaseStock,
			
			-- Sale Stock: sum of negative quantities in date range
			SUM(CASE WHEN TransactionDateTime >= @FromDate AND TransactionDateTime <= @ToDate AND Quantity < 0 
				THEN Quantity ELSE 0 END) AS SaleStock,
			
			-- Monthly Stock: sum of all quantities in date range
			SUM(CASE WHEN TransactionDateTime >= @FromDate AND TransactionDateTime <= @ToDate 
				THEN Quantity ELSE 0 END) AS MonthlyStock,
			
			-- Closing Stock: sum of all quantities up to ToDate
			SUM(CASE WHEN TransactionDateTime <= @ToDate THEN Quantity ELSE 0 END) AS ClosingStock
		FROM [ItemStock] WITH (NOLOCK)
		WHERE GarageId = @GarageId
		GROUP BY ItemId
	),
	-- Calculate average prices for purchases in date range
	PriceAggregates AS (
		SELECT 
			ItemId,
			AVG(CASE WHEN Quantity > 0 AND NetRate IS NOT NULL THEN NetRate ELSE NULL END) AS AveragePrice
		FROM [ItemStock] WITH (NOLOCK)
		WHERE GarageId = @GarageId
			AND TransactionDateTime >= @FromDate 
			AND TransactionDateTime <= @ToDate
		GROUP BY ItemId
	),
	-- Get last purchase price and date for each raw material in date range
	LastPurchaseInfo AS (
		SELECT 
			ItemId,
			NetRate AS LastPurchasePrice,
			ROW_NUMBER() OVER (PARTITION BY ItemId ORDER BY TransactionDateTime DESC, Id DESC) AS RowNum
		FROM [ItemStock] WITH (NOLOCK)
		WHERE GarageId = @GarageId
			AND TransactionDateTime >= @FromDate 
			AND TransactionDateTime <= @ToDate
			AND Quantity > 0
			AND NetRate IS NOT NULL
	)
	-- Final select combining all pre-calculated data
	SELECT
		sa.ItemId,
		i.[Name] ItemName,
		i.Code ItemCode,
		i.ItemTypeId,
		it.[Name] ItemTypeName,
		i.ItemCategoryId,
		ic.[Name] ItemCategoryName,
		i.ManufacturerId,
		m.[Name] ManufacturerName,
		i.UnitOfMeasurement,
		
		ISNULL(sa.OpeningStock, 0) AS OpeningStock,
		ISNULL(sa.PurchaseStock, 0) AS PurchaseStock,
		ISNULL(sa.SaleStock, 0) AS SaleStock,
		ISNULL(sa.MonthlyStock, 0) AS MonthlyStock,
		ISNULL(sa.ClosingStock, 0) AS ClosingStock,
		
		i.ReorderLevel,
		i.Rate,
		ISNULL(i.Rate * sa.ClosingStock, 0) AS ClosingValue,
		
		ISNULL(pa.AveragePrice, 0) AS AveragePrice,
		ISNULL(pa.AveragePrice * sa.ClosingStock, 0) AS WeightedAverageValue,
		
		ISNULL(lpi.LastPurchasePrice, 0) AS LastPurchasePrice,
		ISNULL(lpi.LastPurchasePrice * sa.ClosingStock, 0) AS LastPurchaseValue
		
	FROM StockAggregates sa
	
	LEFT JOIN dbo.Item i WITH (NOLOCK)
		ON i.Id = sa.ItemId

	LEFT JOIN dbo.ItemType it WITH (NOLOCK) 
		ON it.Id = i.ItemTypeId

	LEFT JOIN dbo.ItemCategory ic WITH (NOLOCK) 
		ON ic.Id = i.ItemCategoryId
	
	LEFT JOIN dbo.Manufacturer m WITH (NOLOCK) 
		ON m.Id = i.ManufacturerId

	LEFT JOIN PriceAggregates pa 
		ON pa.ItemId = sa.ItemId
		
	LEFT JOIN LastPurchaseInfo lpi 
		ON lpi.ItemId = sa.ItemId AND lpi.RowNum = 1

	WHERE sa.OpeningStock != 0 
		OR sa.PurchaseStock != 0 
		OR sa.SaleStock != 0 
		OR sa.ClosingStock != 0;
END