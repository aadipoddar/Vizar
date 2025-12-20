CREATE TABLE [dbo].[ServiceDetail]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MasterId] INT NOT NULL, 
    [ServiceTypeId] INT NOT NULL,
    [VehicleId] INT NOT NULL,
    [CurrentHour] MONEY NULL, 
    [CurrentKM] MONEY NULL, 
    [Quantity] MONEY NOT NULL DEFAULT 1,
	[Rate] MONEY NOT NULL,
    [Total] MONEY NOT NULL DEFAULT 0,
    [Remarks] VARCHAR(MAX) NULL,
	[Status] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_ServiceDetail_ToService] FOREIGN KEY ([MasterId]) REFERENCES [Service](Id), 
    CONSTRAINT [FK_ServiceDetail_ToVehicle] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicle](Id),
    CONSTRAINT [FK_ServiceDetail_ToServiceType] FOREIGN KEY ([ServiceTypeId]) REFERENCES [ServiceType](Id)
)
