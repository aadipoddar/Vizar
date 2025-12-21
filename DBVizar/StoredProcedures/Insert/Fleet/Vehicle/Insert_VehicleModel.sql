CREATE PROCEDURE [dbo].[Insert_VehicleModel]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(50),
	@ManufacturerId INT,
	@Remarks VARCHAR(MAX),
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[VehicleModel]
		(
			[Name],
			[Code],
			[ManufacturerId],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@Code,
			@ManufacturerId,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[VehicleModel]
		SET
			[Name] = @Name,
			[Code] = @Code,
			[ManufacturerId] = @ManufacturerId,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END