CREATE PROCEDURE [dbo].[Insert_ItemCategory]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(50),
	@Remarks VARCHAR(MAX) = NULL,
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[ItemCategory]
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
		UPDATE [dbo].[ItemCategory]
		SET
			[Name] = @Name,
			[Code] = @Code,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END