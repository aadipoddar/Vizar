CREATE PROCEDURE [dbo].[Insert_Item]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(10),
	@ItemTypeId INT,
	@ItemCategoryId INT,
	@UnitOfMeasurement VARCHAR(20),
	@PartNo VARCHAR(MAX),
	@ManufacturerId INT,
	@Rate MONEY,
	@TaxId INT,
	@ReorderLevel MONEY,
	@Remarks VARCHAR(MAX),
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
			[UnitOfMeasurement],
			[PartNo],
			[ManufacturerId],
			[Rate],
			[TaxId],
			[ReorderLevel],
			[Remarks],
			[Status]
		)
		VALUES
		(
			@Name,
			@Code,
			@ItemTypeId,
			@ItemCategoryId,
			@UnitOfMeasurement,
			@PartNo,
			@ManufacturerId,
			@Rate,
			@TaxId,
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
			[ItemTypeId] = @ItemTypeId,
			[ItemCategoryId] = @ItemCategoryId,
			[UnitOfMeasurement] = @UnitOfMeasurement,
			[PartNo] = @PartNo,
			[ManufacturerId] = @ManufacturerId,
			[Rate] = @Rate,
			[TaxId] = @TaxId,
			[ReorderLevel] = @ReorderLevel,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END