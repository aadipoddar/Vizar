CREATE PROCEDURE [dbo].[Insert_User]
	@Id INT,
	@Name VARCHAR(250),
	@UserId VARCHAR(250),
	@Password VARCHAR(250),
	@Phone VARCHAR(10) = NULL,
	@Email VARCHAR(250) = NULL,
	@Inventory BIT = 0,
	@Admin BIT = 0,
	@Status BIT = 1
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[User] ([Name], [UserId], [Password], [Phone], [Email], [Inventory], [Admin], [Status])
		VALUES (@Name, @UserId, @Password, @Phone, @Email, @Inventory, @Admin, @Status)
	END
	ELSE
	BEGIN
		UPDATE [dbo].[User]
		SET 
			[Name] = @Name,
			[UserId] = @UserId,
			[Password] = @Password,
			[Phone] = @Phone,
			[Email] = @Email,
			[Inventory] = @Inventory,
			[Admin] = @Admin,
			[Status] = @Status
		WHERE [Id] = @Id
	END
END