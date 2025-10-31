CREATE PROCEDURE [dbo].[Load_PurchaseReturnOverview_By_Date]
	@StartDate DATETIME,
	@EndDate DATETIME
AS
BEGIN
	SELECT *
	FROM PurchaseReturn_Overview
	WHERE TransactionDateTime BETWEEN @StartDate AND @EndDate
END