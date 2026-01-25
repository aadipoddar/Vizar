CREATE TABLE [dbo].[OutsideRepairDetail]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL,
    [Job] VARCHAR(MAX) NOT NULL,
	[Quantity] MONEY NOT NULL DEFAULT 1,
	[Rate] MONEY NOT NULL,
    [Total] MONEY NOT NULL DEFAULT 0,
    [Remarks] VARCHAR(MAX) NULL,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_OutsideRepairDetail_ToItemIssue] FOREIGN KEY ([MasterId]) REFERENCES [OutsideRepair](Id)
)
