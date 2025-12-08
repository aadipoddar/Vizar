CREATE PROCEDURE [dbo].[Insert_User]
	@Id INT OUTPUT,
	@Name VARCHAR(250),
	@Phone VARCHAR(10),
	@Password VARCHAR(250),
	@Email VARCHAR(250) = NULL,
	@Purchase BIT = 0,
	@Accounts BIT = 0,
	@Admin BIT = 0,
	@Remarks VARCHAR(MAX) = NULL,
	@Status BIT = 1,
	@FailedAttempts INT = 0,
	@CodeResends INT = 0,
	@LastCode INT = NULL,
	@LastCodeDateTime DATETIME = NULL
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[User]
		(
			[Name],
			[Password],
			[Phone],
			[Email],
			[Purchase],
			[Accounts],
			[Admin],
			[Status],
			[LastCode],
			[LastCodeDateTime],
			[Remarks],
			[FailedAttempts],
			[CodeResends]
		)
		VALUES
		(
			@Name,
			@Password,
			@Phone,
			@Email,
			@Purchase,
			@Accounts,
			@Admin,
			@Status,
			@LastCode,
			@LastCodeDateTime,
			@Remarks,
			@FailedAttempts,
			@CodeResends
		);

		SET @Id = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [dbo].[User]
		SET 
			[Name] = @Name,
			[Password] = @Password,
			[Phone] = @Phone,
			[Email] = @Email,
			[Purchase] = @Purchase,
			[Accounts] = @Accounts,
			[Admin] = @Admin,
			[Remarks] = @Remarks,
			[Status] = @Status,
			[FailedAttempts] = @FailedAttempts,
			[CodeResends] = @CodeResends,
			[LastCode] = @LastCode,
			[LastCodeDateTime] = @LastCodeDateTime
		WHERE [Id] = @Id
	END

	SELECT @Id AS Id
END