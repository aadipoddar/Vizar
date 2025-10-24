CREATE TABLE [dbo].[User]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(MAX) NOT NULL, 
    [Phone] VARCHAR(10) NOT NULL, 
    [Password] VARCHAR(MAX) NOT NULL,
    [Email] VARCHAR(MAX) NULL, 
    [Inventory] BIT NOT NULL DEFAULT 0, 
    [Admin] BIT NOT NULL DEFAULT 0, 
    [Remarks] VARCHAR(MAX) NULL,
    [Status] BIT NOT NULL DEFAULT 1, 
    [FailedAttempts] INT NOT NULL DEFAULT 0,
    [CodeResends] INT NOT NULL DEFAULT 0,
    [LastCode] INT NULL, 
    [LastCodeDateTime] DATETIME NULL DEFAULT (((getdate() AT TIME ZONE 'UTC') AT TIME ZONE 'India Standard Time')) 
)
