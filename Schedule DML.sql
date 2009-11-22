DROP TABLE Schedules.ScheduleTimes
DROP TABLE Schedules.ScheduleDates

CREATE TABLE Schedules.ScheduleDates (
	ID				UNIQUEIDENTIFIER	ROWGUIDCOL	CONSTRAINT ScheduleDatesKey		PRIMARY KEY DEFAULT newid(),
	[Date]			DATETIME			NOT NULL,--	CONSTRAINT ScheduleDatesUnique	UNIQUE,
	Title			NVARCHAR(128)		NOT NULL
)
CREATE TABLE Schedules.ScheduleTimes (
	ID				UNIQUEIDENTIFIER	ROWGUIDCOL	CONSTRAINT ScheduleTimesKey		PRIMARY KEY DEFAULT newid(),
	CellID			UNIQUEIDENTIFIER	NOT NULL	CONSTRAINT ScheduleTimesForeignKey REFERENCES Schedules.ScheduleDates(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	[Name]			NVARCHAR(64)		NOT NULL,
	[Time]			DATETIME			NOT NULL,
	IsBold			BIT					NOT NULL	DEFAULT 0,
	LastModified	DATETIME			NOT NULL	DEFAULT getdate()
)