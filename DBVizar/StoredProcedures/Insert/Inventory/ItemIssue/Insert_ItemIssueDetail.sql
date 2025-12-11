CREATE PROCEDURE [dbo].[Insert_ItemIssueDetail]
	@Id INT OUTPUT,
	@MasterId INT,
	@ItemId INT,
	@VehicleId INT,
	@CurrentHour MONEY,
	@CurrentKM MONEY,
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
		INSERT INTO [dbo].[ItemIssueDetail]
		(
			MasterId,
			ItemId,
			VehicleId,
			CurrentHour,
			CurrentKM,
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
			@VehicleId,
			@CurrentHour,
			@CurrentKM,
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
		UPDATE [dbo].[ItemIssueDetail]
		SET MasterId = @MasterId,
			ItemId = @ItemId,
			VehicleId = @VehicleId,
			CurrentHour = @CurrentHour,
			CurrentKM = @CurrentKM,
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