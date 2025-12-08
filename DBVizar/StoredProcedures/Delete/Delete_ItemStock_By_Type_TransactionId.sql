CREATE PROCEDURE [dbo].[Delete_ItemStock_By_Type_TransactionId]
	@Type VARCHAR(20),
	@TransactionId INT
AS
BEGIN
	DELETE FROM [dbo].[ItemStock]
	WHERE [Type] = @Type
	  AND [TransactionId] = @TransactionId
END