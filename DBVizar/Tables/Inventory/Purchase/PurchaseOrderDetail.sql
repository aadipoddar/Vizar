CREATE TABLE [dbo].[PurchaseOrderDetail]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL, 
    [ItemId] INT NOT NULL, 
    [UnitOfMeasurement] VARCHAR(20) NOT NULL,
    [Quantity] MONEY NOT NULL DEFAULT 1, 
    [Remarks] VARCHAR(MAX) NULL,
    [Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_PurchaseOrderDetail_ToPurchaseOrder] FOREIGN KEY ([MasterId]) REFERENCES [PurchaseOrder](Id), 
    CONSTRAINT [FK_PurchaseOrderDetail_ToItem] FOREIGN KEY ([ItemId]) REFERENCES [Item](Id)
)
