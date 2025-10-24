CREATE TABLE [dbo].[Item]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(500) NOT NULL UNIQUE,
	[Code] VARCHAR(50) NOT NULL UNIQUE,
	[ItemType] INT NOT NULL,
	[ItemCategory] INT NOT NULL,
	[ManufacturerId] INT NULL,
	[Rate] MONEY NOT NULL,
	[TaxId] INT NOT NULL,
	[UnitOfMeasurement] VARCHAR(20) NOT NULL,
	[ReorderLevel] DECIMAL(7, 3) NULL,
	[Remarks] VARCHAR(MAX) NULL,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_Item_ToManufacturer] FOREIGN KEY ([ManufacturerId]) REFERENCES [dbo].[Manufacturer]([Id]), 
    CONSTRAINT [FK_Item_ToTax] FOREIGN KEY ([TaxId]) REFERENCES [dbo].[Tax]([Id])
)
