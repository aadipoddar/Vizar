CREATE PROCEDURE [dbo].[Insert_ItemStock]
	@Id INT OUTPUT,
	@ItemId INT, 
	@IdentificationNo VARCHAR(MAX),
	@Quantity MONEY, 
	@NetRate MONEY,
	@Type VARCHAR(20), 
	@TransactionId INT,
	@TransactionNo VARCHAR(MAX),
	@TransactionDateTime DATETIME
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[ItemStock] 
		(
			[ItemId], 
			[IdentificationNo],
			[Quantity], 
			[NetRate],
			[Type], 
			[TransactionId],
			[TransactionNo],
			[TransactionDateTime]
		)
		VALUES
		(
			@ItemId, 
			@IdentificationNo,
			@Quantity, 
			@NetRate,
			@Type, 
			@TransactionId,
			@TransactionNo,
			@TransactionDateTime
		);

		SET @Id = SCOPE_IDENTITY();
	END
	ELSE

	BEGIN
		UPDATE [dbo].[ItemStock]
		SET 
			[ItemId] = @ItemId, 
			[IdentificationNo] = @IdentificationNo,
			[Quantity] = @Quantity, 
			[NetRate] = @NetRate,
			[Type] = @Type, 
			[TransactionId] = @TransactionId,
			[TransactionNo] = @TransactionNo,
			[TransactionDateTime] = @TransactionDateTime
		WHERE [Id] = @Id;
	END

	SELECT @Id AS Id;
END;