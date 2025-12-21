CREATE PROCEDURE [dbo].[Insert_Document]
	@Id INT OUTPUT,
	@TransactionNo VARCHAR(500),
	@TransactionDateTime DATETIME,
	@FinancialYearId INT,
	@DocumentTypeId INT,
	@VehicleId INT,
	@CurrentHour MONEY,
	@CurrentKM MONEY,
	@Rate MONEY,
	@RenewalDate DATETIME,
	@Remarks VARCHAR(MAX),
	@DocumentUrl VARCHAR(MAX) = NULL,
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
		INSERT INTO [dbo].[Document]
		(
			[TransactionNo],
			[TransactionDateTime],
			[FinancialYearId],
			[DocumentTypeId],
			[VehicleId],
			[CurrentHour],
			[CurrentKM],
			[Rate],
			[RenewalDate],
			[Remarks],
			[DocumentUrl],
			[CreatedBy],
			[CreatedFromPlatform],
			[Status]
		)
		VALUES
		(
			@TransactionNo,
			@TransactionDateTime,
			@FinancialYearId,
			@DocumentTypeId,
			@VehicleId,
			@CurrentHour,
			@CurrentKM,
			@Rate,
			@RenewalDate,
			@Remarks,
			@DocumentUrl,
			@CreatedBy,
			@CreatedFromPlatform,
			@Status
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[Document]
		SET
			[TransactionNo] = @TransactionNo,
			[TransactionDateTime] = @TransactionDateTime,
			[FinancialYearId] = @FinancialYearId,
			[DocumentTypeId] = @DocumentTypeId,
			[VehicleId] = @VehicleId,
			[CurrentHour] = @CurrentHour,
			[CurrentKM] = @CurrentKM,
			[Rate] = @Rate,
			[RenewalDate] = @RenewalDate,
			[Remarks] = @Remarks,
			[DocumentUrl] = @DocumentUrl,
			[Status] = @Status,
			[LastModifiedBy] = @LastModifiedBy,
			[LastModifiedAt] = @LastModifiedAt,
			[LastModifiedFromPlatform] = @LastModifiedFromPlatform
		WHERE
			[Id] = @Id;
	END

	SELECT @Id AS Id;
END