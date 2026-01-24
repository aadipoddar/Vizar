CREATE TABLE [dbo].[ItemStock]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ItemId] INT NOT NULL,
    [IdentificationNo] VARCHAR(MAX) NULL,
    [Quantity] MONEY NOT NULL,
    [NetRate] MONEY NULL, 
    [Type] VARCHAR(20) NOT NULL, 
    [TransactionId] INT NULL, 
    [TransactionNo] VARCHAR(MAX) NOT NULL, 
    [TransactionDateTime] DATETIME NOT NULL, 
    CONSTRAINT [FK_ItemStock_ToItem] FOREIGN KEY (ItemId) REFERENCES [Item](Id)
)
