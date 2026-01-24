CREATE PROCEDURE [dbo].[Insert_ItemType]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(10),
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[ItemType]
		(
			[Name],
			[Code],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@Code,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[ItemType]
		SET
			[Name] = @Name,
			[Code] = @Code,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END