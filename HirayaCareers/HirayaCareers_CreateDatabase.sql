-- HirayaCareers
-- SQL script for creating the database tables
-- Run this script in an empty Azure SQL database.

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(MAX) NOT NULL,
    LastName NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL
);

CREATE TABLE dbo.Jobs
(
    JobId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(MAX) NOT NULL,
    CompanyName NVARCHAR(MAX) NOT NULL,
    Location NVARCHAR(MAX) NOT NULL,
    EmploymentType NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Requirements NVARCHAR(MAX) NOT NULL,
    DatePosted DATETIME2(7) NOT NULL,
    EmployerId INT NOT NULL,

    CONSTRAINT FK_Jobs_Users
        FOREIGN KEY (EmployerId)
        REFERENCES dbo.Users(UserId)
);

CREATE TABLE dbo.JobApplications
(
    JobApplicationId INT IDENTITY(1,1) PRIMARY KEY,
    JobId INT NOT NULL,
    UserId INT NOT NULL,
    CoverLetter NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(MAX) NOT NULL,
    AppliedAt DATETIME2(7) NOT NULL,
    ResumePath NVARCHAR(MAX) NULL,

    CONSTRAINT FK_JobApplications_Jobs
        FOREIGN KEY (JobId)
        REFERENCES dbo.Jobs(JobId),

    CONSTRAINT FK_JobApplications_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(UserId)
);