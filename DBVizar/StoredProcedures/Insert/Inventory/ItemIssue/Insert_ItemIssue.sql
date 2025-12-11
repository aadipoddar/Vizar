CREATE PROCEDURE [dbo].[Insert_ItemIssue]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(100),
	@CompanyId INT,
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@GarageId INT,
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
		INSERT INTO [dbo].[ItemIssue]
		(
			[TransactionNo],
			[CompanyId],
			[TransactionDateTime],
			[FinancialYearId],
			[GarageId],
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
			@GarageId,
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
		UPDATE [dbo].[ItemIssue]
		SET
			[CompanyId] = @CompanyId,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[GarageId] = @GarageId,
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