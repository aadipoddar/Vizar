CREATE TABLE [dbo].[ItemIssueDetail]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL, 
    [ItemId] INT NOT NULL,
    [VehicleId] INT NULL,
    [CurrentHour] MONEY NULL, 
    [CurrentKM] MONEY NULL, 
    [IdentificationNo] VARCHAR(MAX) NULL,
	[UnitOfMeasurement] VARCHAR(20) NOT NULL,
	[Quantity] MONEY NOT NULL DEFAULT 1,
	[Rate] MONEY NOT NULL,
    [Total] MONEY NOT NULL DEFAULT 0,
    [Remarks] VARCHAR(MAX) NULL,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_ItemIssueDetail_ToItemIssue] FOREIGN KEY ([MasterId]) REFERENCES [ItemIssue](Id), 
    CONSTRAINT [FK_ItemIssueDetail_ToVehicle] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle](Id),
    CONSTRAINT [FK_ItemIssueDetail_ToItem] FOREIGN KEY ([ItemId]) REFERENCES [Item](Id)
)
