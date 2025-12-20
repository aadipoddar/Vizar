CREATE PROCEDURE [dbo].[Insert_ServiceSchedule]
	@Id INT OUTPUT,
	@ServiceTypeId INT,
	@VehicleTypeId INT,
	@IntervalDays INT,
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[ServiceSchedule]
		(
			[ServiceTypeId],
			[VehicleTypeId],
			[IntervalDays],
			[Status]
		)
		VALUES
		(
			@ServiceTypeId,
			@VehicleTypeId,
			@IntervalDays,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[ServiceSchedule]
		SET
			[ServiceTypeId] = @ServiceTypeId,
			[VehicleTypeId] = @VehicleTypeId,
			[IntervalDays] = @IntervalDays,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id
END