CREATE PROCEDURE [dbo].[Insert_DocumentType]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(10),
	@Rate MONEY,
	@Remarks VARCHAR(MAX) = NULL,
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[DocumentType]
		(
			[Name],
			[Code],
			[Rate],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@Code,
			@Rate,
			@Remarks,
			@Status
		);
		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[DocumentType]
		SET
			[Name] = @Name,
			[Code] = @Code,
			[Rate] = @Rate,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE
			[Id] = @Id;
	END

	SELECT @Id AS Id;
END