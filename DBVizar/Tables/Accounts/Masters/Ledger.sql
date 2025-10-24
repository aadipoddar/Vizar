CREATE TABLE [dbo].[Ledger]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(500) NOT NULL UNIQUE, 
    [GroupId] INT NOT NULL, 
    [AccountTypeId] INT NOT NULL, 
    [Code] VARCHAR(20) NOT NULL UNIQUE, 
    [StateUTId] INT NULL, 
    [GSTNo] VARCHAR(MAX) NULL, 
    [Alias] VARCHAR(MAX) NULL, 
    [Phone] VARCHAR(10) NULL, 
    [Email] VARCHAR(MAX) NULL, 
    [Address] VARCHAR(MAX) NULL, 
    [Remarks] VARCHAR(MAX) NULL, 
    [Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_Ledger_ToGroup] FOREIGN KEY (GroupId) REFERENCES [Group](Id), 
    CONSTRAINT [FK_Ledger_ToAccountType] FOREIGN KEY (AccountTypeId) REFERENCES [AccountType](Id), 
    CONSTRAINT [FK_Ledger_ToStateUT] FOREIGN KEY ([StateUTId]) REFERENCES [StateUT](Id)
)