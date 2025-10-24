CREATE TABLE [dbo].[PurchaseDetail]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PurchaseId] INT NOT NULL,
	[ItemId] INT NOT NULL,
	[IdentificationNo] VARCHAR(MAX) NULL,
	[Quantity] DECIMAL(7, 3) NOT NULL DEFAULT 1,
	[UnitOfMeasurement] VARCHAR(20) NOT NULL,
	[Rate] MONEY NOT NULL,
	[BaseTotal] MONEY NOT NULL DEFAULT 0,
	[DiscountPercent] DECIMAL(5, 2) NOT NULL DEFAULT 0,
	[DiscountAmount] MONEY NOT NULL DEFAULT 0,
	[AfterDiscount] MONEY NOT NULL DEFAULT 0,
	[TaxId] INT NOT NULL,
	[TaxAmount] MONEY NOT NULL DEFAULT 0,
	[Total] MONEY NOT NULL DEFAULT 0,
	[NetRate] MONEY NOT NULL DEFAULT 0,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_PurchaseDetail_ToPurchase] FOREIGN KEY ([PurchaseId]) REFERENCES [Purchase]([Id]),
	CONSTRAINT [FK_PurchaseDetail_ToItem] FOREIGN KEY ([ItemId]) REFERENCES [Item]([Id]), 
    CONSTRAINT [FK_PurchaseDetail_ToTax] FOREIGN KEY ([TaxId]) REFERENCES [Tax]([Id])
)
