CREATE PROCEDURE [dbo].[Insert_InsideRepairDetail]
	@Id INT OUTPUT,
	@MasterId INT,
	@ItemId INT,
	@IdentificationNo VARCHAR(MAX),
	@UnitOfMeasurement VARCHAR(20),
	@Quantity MONEY,
	@Rate MONEY,
	@Total MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[InsideRepairDetail]
		(
			MasterId,
			ItemId,
			IdentificationNo,
			UnitOfMeasurement,
			Quantity,
			Rate,
			Total,
			Remarks,
			Status
		)
		VALUES
		(
			@MasterId,
			@ItemId,
			@IdentificationNo,
			@UnitOfMeasurement,
			@Quantity,
			@Rate,
			@Total,
			@Remarks,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].[InsideRepairDetail]
		SET MasterId = @MasterId,
			ItemId = @ItemId,
			IdentificationNo = @IdentificationNo,
			UnitOfMeasurement = @UnitOfMeasurement,
			Quantity = @Quantity,
			Rate = @Rate,
			Total = @Total,
			Remarks = @Remarks,
			Status = @Status
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END