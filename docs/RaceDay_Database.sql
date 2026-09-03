/* =========================================================
   RaceDay Event Management System
   PROG6212 Part 1 - SQL Database Script
   ========================================================= */

USE master;
GO

IF DB_ID('RaceDay') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END;
GO

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

/* =========================================================
   TABLE 1: Role
   ========================================================= */
CREATE TABLE Role (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

/* =========================================================
   TABLE 2: AppUser
   ========================================================= */
CREATE TABLE AppUser (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    DateRegistered DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_AppUser_Role
        FOREIGN KEY (RoleID)
        REFERENCES Role(RoleID)
);

/* =========================================================
   TABLE 3: Event
   ========================================================= */
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    EventDate DATE NOT NULL,
    EventLocation VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES AppUser(UserID)
);

/* =========================================================
   TABLE 4: Category
   ========================================================= */
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    CategoryDescription VARCHAR(300) NULL
);

/* =========================================================
   TABLE 5: EventCategory
   ========================================================= */
CREATE TABLE EventCategory (
    EventCategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,

    CONSTRAINT FK_EventCategory_Event
        FOREIGN KEY (EventID)
        REFERENCES Event(EventID),

    CONSTRAINT FK_EventCategory_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Category(CategoryID),

    CONSTRAINT UQ_EventCategory
        UNIQUE (EventID, CategoryID)
);

/* =========================================================
   TABLE 6: Enrolment
   ========================================================= */
CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    EventCategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL DEFAULT 'Active',

    CONSTRAINT FK_Enrolment_User
        FOREIGN KEY (UserID)
        REFERENCES AppUser(UserID),

    CONSTRAINT FK_Enrolment_EventCategory
        FOREIGN KEY (EventCategoryID)
        REFERENCES EventCategory(EventCategoryID),

    CONSTRAINT UQ_Enrolment
        UNIQUE (UserID, EventCategoryID),

    CONSTRAINT CK_Enrolment_Status
        CHECK (Status IN ('Active', 'Cancelled', 'Completed'))
);

/* =========================================================
   TABLE 7: Result
   ========================================================= */
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    Position INT NULL,
    FinishTime TIME NULL,
    Points DECIMAL(6,2) NULL DEFAULT 0,
    DateCaptured DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Result_Enrolment
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolment(EnrolmentID),

    CONSTRAINT CK_Result_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_Result_Points
        CHECK (Points IS NULL OR Points >= 0)
);

GO

/* =========================================================
   SEED DATA
   ========================================================= */

/* Roles */
INSERT INTO Role (RoleName)
VALUES
('Organiser'),
('Participant');

/* Users
   RoleID 1 = Organiser
   RoleID 2 = Participant
*/
INSERT INTO AppUser
(RoleID, FirstName, LastName, Email, PasswordHash, PhoneNumber)
VALUES
(1, 'Thabo', 'Mokoena', 'thabo@raceday.co.za', 'HASHED_PASSWORD_1', '0821111111'),
(1, 'Naledi', 'Jacobs', 'naledi@raceday.co.za', 'HASHED_PASSWORD_2', '0822222222'),
(2, 'Sipho', 'Dlamini', 'sipho@example.com', 'HASHED_PASSWORD_3', '0831111111'),
(2, 'Ayesha', 'Khan', 'ayesha@example.com', 'HASHED_PASSWORD_4', '0832222222');

/* Events */
INSERT INTO Event
(OrganiserID, EventName, EventDate, EventLocation, Description)
VALUES
(1, 'Johannesburg City Run', '2026-10-10', 'Johannesburg, Gauteng',
 'Road running event through central Johannesburg.'),

(1, 'Cape Town Coastal Cycle', '2026-11-15', 'Cape Town, Western Cape',
 'Cycling event along the Cape Town coastline.'),

(2, 'Durban Summer Walk', '2026-12-05', 'Durban, KwaZulu-Natal',
 'Community walking event for participants of different fitness levels.');

/* Categories */
INSERT INTO Category
(CategoryName, CategoryDescription)
VALUES
('5km Run', 'Five kilometre running category'),
('10km Run', 'Ten kilometre running category'),
('21km Run', 'Half marathon running category'),
('40km Cycle', 'Forty kilometre cycling category'),
('100km Cycle', 'One hundred kilometre cycling category'),
('5km Walk', 'Five kilometre walking category');

/* Event categories */
INSERT INTO EventCategory (EventID, CategoryID)
VALUES
(1, 1),
(1, 2),
(1, 3),
(2, 4),
(2, 5),
(3, 6);

/* Sample enrolments */
INSERT INTO Enrolment
(UserID, EventCategoryID, Status)
VALUES
(3, 1, 'Completed'),
(3, 4, 'Active'),
(4, 2, 'Completed'),
(4, 6, 'Active');

/* Sample results */
INSERT INTO Result
(EnrolmentID, Position, FinishTime, Points)
VALUES
(1, 12, '00:28:45', 80.00),
(3, 7, '00:58:12', 90.00);

GO

/* =========================================================
   VERIFICATION QUERIES
   ========================================================= */

SELECT * FROM Role;
SELECT * FROM AppUser;
SELECT * FROM Event;
SELECT * FROM Category;
SELECT * FROM EventCategory;
SELECT * FROM Enrolment;
SELECT * FROM Result;

GO