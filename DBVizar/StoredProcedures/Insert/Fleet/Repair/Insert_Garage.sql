CREATE PROCEDURE [dbo].[Insert_Garage]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@External BIT,
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[Garage]
		(
			[Name],
			[External],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@External,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[Garage]
		SET
			[Name] = @Name,
			[External] = @External,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END