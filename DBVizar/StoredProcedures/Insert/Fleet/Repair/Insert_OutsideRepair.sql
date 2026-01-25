CREATE PROCEDURE [dbo].[Insert_OutsideRepair]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(100),
	@CompanyId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@VendorId INT,
	@VehicleId INT,
	@CurrentHour MONEY,
	@CurrentKM MONEY,
	@ApprovedBy VARCHAR(MAX),
	@TotalItems INT,
	@TotalQuantity MONEY,
	@TotalAmount MONEY,
	@Remarks VARCHAR(MAX),
	@CreatedBy INT,
	@CreatedAt DATETIME,
	@CreatedFromPlatform VARCHAR(MAX),
	@Status BIT,
	@LastModifiedBy INT,
	@LastModifiedAt DATETIME,
	@LastModifiedFromPlatform VARCHAR(MAX)
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[OutsideRepair]
		(
			[TransactionNo],
			[CompanyId],
			[TransactionDateTime],
			[FinancialYearId],
			[VendorId],
			[VehicleId],
			[CurrentHour],
			[CurrentKM],
			[ApprovedBy],
			[TotalItems],
			[TotalQuantity],
			[TotalAmount],
			[Remarks],
			[CreatedBy],
			[CreatedFromPlatform],
			[Status]
		)
		VALUES
		(
			@TransactionNo,
			@CompanyId,
			@TransactionDateTime,
			@FinancialYearId,
			@VendorId,
			@VehicleId,
			@CurrentHour,
			@CurrentKM,
			@ApprovedBy,
			@TotalItems,
			@TotalQuantity,
			@TotalAmount,
			@Remarks,
			@CreatedBy,
			@CreatedFromPlatform,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[OutsideRepair]
		SET
			[CompanyId] = @CompanyId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[VendorId] = @VendorId,
			[VehicleId] = @VehicleId,
			[CurrentHour] = @CurrentHour,
			[CurrentKM] = @CurrentKM,
			[ApprovedBy] = @ApprovedBy,
			[TotalItems] = @TotalItems,
			[TotalQuantity] = @TotalQuantity,
			[TotalAmount] = @TotalAmount,
			[Remarks] = @Remarks,
			[Status] = @Status,
			[LastModifiedBy] = @LastModifiedBy,
			[LastModifiedAt] = @LastModifiedAt,
			[LastModifiedFromPlatform] = @LastModifiedFromPlatform
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END