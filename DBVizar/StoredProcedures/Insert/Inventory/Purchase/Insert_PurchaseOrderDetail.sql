CREATE PROCEDURE [dbo].[Insert_PurchaseOrderDetail]
	@Id INT OUTPUT,
	@MasterId INT, 
	@ItemId INT, 
	@UnitOfMeasurement VARCHAR(20),
	@Quantity MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[PurchaseOrderDetail]
		(
			[MasterId], 
			[ItemId],
			[UnitOfMeasurement],
			[Quantity], 
			[Remarks],
			[Status]
		)
		VALUES
		(
			@MasterId, 
			@ItemId, 
			@UnitOfMeasurement,
			@Quantity, 
			@Remarks,
			@Status
		);
		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[PurchaseOrderDetail]
		SET 
			[MasterId] = @MasterId, 
			[ItemId] = @ItemId,
			[UnitOfMeasurement] = @UnitOfMeasurement,
			[Quantity] = @Quantity, 
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END;