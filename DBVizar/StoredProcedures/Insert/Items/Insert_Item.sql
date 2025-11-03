CREATE PROCEDURE [dbo].[Insert_Item]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(50),
	@ItemType INT,
	@ItemCategory INT,
	@ManufacturerId INT = NULL,
	@Rate MONEY,
	@TaxId INT,
	@UnitOfMeasurement VARCHAR(20),
	@ReorderLevel DECIMAL(7, 3) = NULL,
	@Remarks VARCHAR(MAX) = NULL,
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[Item]
		(
			[Name],
			[Code],
			[ItemTypeId],
			[ItemCategoryId],
			[ManufacturerId],
			[Rate],
			[TaxId],
			[UnitOfMeasurement],
			[ReorderLevel],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@Code,
			@ItemType,
			@ItemCategory,
			@ManufacturerId,
			@Rate,
			@TaxId,
			@UnitOfMeasurement,
			@ReorderLevel,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[Item]
		SET
			[Name] = @Name,
			[Code] = @Code,
			[ItemTypeId] = @ItemType,
			[ItemCategoryId] = @ItemCategory,
			[ManufacturerId] = @ManufacturerId,
			[Rate] = @Rate,
			[TaxId] = @TaxId,
			[UnitOfMeasurement] = @UnitOfMeasurement,
			[ReorderLevel] = @ReorderLevel,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END