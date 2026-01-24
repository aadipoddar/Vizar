CREATE TABLE [dbo].[Item]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(500) NOT NULL UNIQUE,
	[Code] VARCHAR(10) NOT NULL UNIQUE,
	[ItemTypeId] INT NOT NULL,
	[ItemCategoryId] INT NOT NULL,
	[UnitOfMeasurement] VARCHAR(20) NOT NULL,
    [PartNo] VARCHAR(MAX) NULL, 
	[ManufacturerId] INT NULL,
	[Rate] MONEY NOT NULL,
	[TaxId] INT NULL,
	[ReorderLevel] MONEY NULL DEFAULT 0,
	[Remarks] VARCHAR(MAX) NULL,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_Item_ToItemType] FOREIGN KEY ([ItemTypeId]) REFERENCES [dbo].[ItemType]([Id]), 
    CONSTRAINT [FK_Item_ToItemCategory] FOREIGN KEY ([ItemCategoryId]) REFERENCES [dbo].[ItemCategory]([Id]),
    CONSTRAINT [FK_Item_ToManufacturer] FOREIGN KEY ([ManufacturerId]) REFERENCES [dbo].[Manufacturer]([Id]), 
    CONSTRAINT [FK_Item_ToTax] FOREIGN KEY ([TaxId]) REFERENCES [dbo].[Tax]([Id])
)
