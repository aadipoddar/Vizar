CREATE PROCEDURE [dbo].[Delete_ItemStock_By_Id]
	@Id INT
AS
BEGIN
	DELETE FROM [dbo].[ItemStock]
	WHERE [Id] = @Id
END