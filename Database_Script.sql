USE [BeanSceneDB]
GO

-- Drop tables if they exist
DROP TABLE IF EXISTS Reservation;
DROP TABLE IF EXISTS Sitting;
DROP TABLE IF EXISTS [Table];
DROP TABLE IF EXISTS [User];

-- Create tables
CREATE TABLE Sitting (
    SittingId INT IDENTITY(1,1) PRIMARY KEY,
    EndTime DATETIME2 NOT NULL,
    SittingStatus INT NOT NULL,
    SittingType INT NOT NULL,
    StartTime DATETIME2 NOT NULL
);

CREATE TABLE [Table] (
    TableId INT IDENTITY(1,1) PRIMARY KEY,
    Area INT NOT NULL,
    TableName AS (
        CASE Area
            WHEN 0 THEN 'M'
            WHEN 1 THEN 'O'
            WHEN 2 THEN 'B'
        END + CAST(TableNo AS VARCHAR)
    ) PERSISTED,
    TableNo INT NOT NULL
);

CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(450) NOT NULL,
    FirstName NVARCHAR(15) NOT NULL,
    LastName NVARCHAR(15) NOT NULL,
    Membership INT NOT NULL,
    Phone NVARCHAR(450) NOT NULL
);

CREATE UNIQUE INDEX IX_User_Email ON [User] (Email);
CREATE UNIQUE INDEX IX_User_Phone ON [User] (Phone);

CREATE TABLE Reservation (
    ResId INT IDENTITY(1,1) PRIMARY KEY,
    Date DATETIME2 NOT NULL,
    Duration INT NOT NULL,
    NumOfGuests INT NOT NULL,
    ResStartTime DATETIME2 NOT NULL,
    ResStatus INT NOT NULL,
    SittingId INT NOT NULL,
    Source INT NOT NULL,
    SpecialReqs NVARCHAR(MAX),
    TableId INT NOT NULL,
    UserId INT NOT NULL
);

CREATE INDEX IX_Reservation_SittingId ON Reservation (SittingId);
CREATE INDEX IX_Reservation_TableId ON Reservation (TableId);
CREATE INDEX IX_Reservation_UserId ON Reservation (UserId);

-- Foreign key constraints for Reservation table
ALTER TABLE Reservation
ADD CONSTRAINT FK_Reservation_SittingId FOREIGN KEY (SittingId) REFERENCES Sitting (SittingId) ON DELETE CASCADE;
GO

ALTER TABLE Reservation
ADD CONSTRAINT FK_Reservation_TableId FOREIGN KEY (TableId) REFERENCES [Table] (TableId) ON DELETE CASCADE;
GO

ALTER TABLE Reservation
ADD CONSTRAINT FK_Reservation_UserId FOREIGN KEY (UserId) REFERENCES [User] (UserId) ON DELETE CASCADE;
GO

