CREATE PROCEDURE [dbo].[Insert_Tax]
	@Id INT OUTPUT,
	@Name VARCHAR(500),
	@Code VARCHAR(10),
	@CGST DECIMAL(5, 2),
	@SGST DECIMAL(5, 2),
	@IGST DECIMAL(5, 2),
	@Inclusive BIT,
	@Extra BIT,
	@Remarks VARCHAR(MAX),
	@Status BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[Tax] ([Name], [Code], [CGST], [SGST], [IGST], [Inclusive], [Extra], [Remarks], [Status])
		VALUES (@Name, @Code, @CGST, @SGST, @IGST, @Inclusive, @Extra, @Remarks, @Status);

		SET @Id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Tax]
		SET [Name] = @Name,
			[Code] = @Code, 
			[CGST] = @CGST, 
			[SGST] = @SGST, 
			[IGST] = @IGST, 
			[Inclusive] = @Inclusive,
			[Extra] = @Extra,
			[Remarks] = @Remarks,
			[Status] = @Status
		WHERE [Id] = @Id;
	END

	SELECT @Id AS 'Id';
END