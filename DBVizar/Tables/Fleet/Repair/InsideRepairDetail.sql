CREATE TABLE [dbo].[InsideRepairDetail]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL, 
    [ItemId] INT NOT NULL,
    [IdentificationNo] VARCHAR(MAX) NULL,
	[UnitOfMeasurement] VARCHAR(20) NOT NULL,
	[Quantity] MONEY NOT NULL DEFAULT 1,
	[Rate] MONEY NOT NULL,
    [Total] MONEY NOT NULL DEFAULT 0,
    [Remarks] VARCHAR(MAX) NULL,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_InsideRepairDetail_ToItemIssue] FOREIGN KEY ([MasterId]) REFERENCES [InsideRepair](Id), 
    CONSTRAINT [FK_InsideRepairDetail_ToItem] FOREIGN KEY ([ItemId]) REFERENCES [Item](Id)
)
