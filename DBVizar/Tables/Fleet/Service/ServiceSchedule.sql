CREATE TABLE [dbo].[ServiceSchedule]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ServiceTypeId] INT NOT NULL, 
    [VehicleTypeId] INT NOT NULL, 
    [IntervalDays] INT NOT NULL, 
    [Status] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [FK_ServiceSchedule_ToServiceType] FOREIGN KEY ([ServiceTypeId]) REFERENCES [ServiceType]([Id]),
    CONSTRAINT [FK_ServiceSchedule_ToVehicleType] FOREIGN KEY ([VehicleTypeId]) REFERENCES [VehicleType]([Id])
)
