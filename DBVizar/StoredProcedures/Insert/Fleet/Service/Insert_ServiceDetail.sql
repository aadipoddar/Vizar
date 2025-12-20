CREATE PROCEDURE [dbo].[Insert_ServiceDetail]
	@Id INT OUTPUT,
	@MasterId INT,
	@ServiceTypeId INT,
	@VehicleId INT,
	@CurrentHour MONEY,
	@CurrentKM MONEY,
	@Quantity MONEY,
	@Rate MONEY,
	@Total MONEY,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[ServiceDetail]
		(
			MasterId,
			ServiceTypeId,
			VehicleId,
			CurrentHour,
			CurrentKM,
			Quantity,
			Rate,
			Total,
			Remarks,
			Status
		)
		VALUES
		(
			@MasterId,
			@ServiceTypeId,
			@VehicleId,
			@CurrentHour,
			@CurrentKM,
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
		UPDATE [dbo].[ServiceDetail]
		SET MasterId = @MasterId,
			ServiceTypeId = @ServiceTypeId,
			VehicleId = @VehicleId,
			CurrentHour = @CurrentHour,
			CurrentKM = @CurrentKM,
			Quantity = @Quantity,
			Rate = @Rate,
			Total = @Total,
			Remarks = @Remarks,
			Status = @Status
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END