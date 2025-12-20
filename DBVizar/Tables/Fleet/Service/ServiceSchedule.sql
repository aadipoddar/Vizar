CREATE TABLE [dbo].[ServiceSchedule]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ServiceTypeId] INT NOT NULL, 
    [VehicleTypeId] INT NOT NULL, 
    [IntervalDays] INT NOT NULL, 
    [Status] BIT NOT NULL DEFAULT 1
)
