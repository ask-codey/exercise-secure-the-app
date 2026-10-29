-- Run this once in SSMS, in your Northwind database, to add the users table.
CREATE TABLE Users (
    UserId int IDENTITY PRIMARY KEY,
    UserName nvarchar(50) NOT NULL UNIQUE,
    PasswordHash nvarchar(100) NOT NULL,
    Role nvarchar(20) NOT NULL DEFAULT 'User'
);
