CREATE PROCEDURE [dbo].[Insert_PurchaseOrder]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(MAX),
	@CompanyId INT,
	@VendorId INT,
	@GarageId INT,
	@PurchaseId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@TotalItems INT,
	@TotalQuantity MONEY,
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
		INSERT INTO [dbo].[PurchaseOrder]
		(
			[TransactionNo],
			[CompanyId],
			[VendorId],
			[GarageId],
			[PurchaseId],
			[TransactionDateTime],
			[FinancialYearId],
			[TotalItems],
			[TotalQuantity],
			[Remarks],
			[CreatedBy],
			[CreatedAt],
			[CreatedFromPlatform],
			[Status]
		)
		VALUES
		(
			@TransactionNo,
			@CompanyId,
			@VendorId,
			@GarageId,
			@PurchaseId,
			@TransactionDateTime,
			@FinancialYearId,
			@TotalItems,
			@TotalQuantity,
			@Remarks,
			@CreatedBy,
			@CreatedAt,
			@CreatedFromPlatform,
			@Status
		)
		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[PurchaseOrder]
		SET
			TransactionNo = @TransactionNo,
			[CompanyId] = @CompanyId,
			[VendorId] = @VendorId,
			[GarageId] = @GarageId,
			[PurchaseId] = @PurchaseId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[TotalItems] = @TotalItems,
			[TotalQuantity] = @TotalQuantity,
			[Remarks] = @Remarks,
			[Status] = @Status,
			[LastModifiedBy] = @LastModifiedBy,
			[LastModifiedAt] = @LastModifiedAt,
			[LastModifiedFromPlatform] = @LastModifiedFromPlatform
		WHERE Id = @Id;
	END

	SELECT @Id AS Id;
END