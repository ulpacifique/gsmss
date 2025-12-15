-- Script to remove invalid shadow property columns from Contributions table
-- Run this script directly on your SQL Server database

USE [YourDatabaseName]; -- Replace with your actual database name
GO

-- Drop SavingsGoalGoalId column if it exists
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contributions') AND name = 'SavingsGoalGoalId')
BEGIN
    ALTER TABLE [Contributions] DROP COLUMN [SavingsGoalGoalId];
    PRINT 'Dropped column SavingsGoalGoalId';
END
ELSE
BEGIN
    PRINT 'Column SavingsGoalGoalId does not exist';
END
GO

-- Drop UserId1 column if it exists
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Contributions') AND name = 'UserId1')
BEGIN
    ALTER TABLE [Contributions] DROP COLUMN [UserId1];
    PRINT 'Dropped column UserId1';
END
ELSE
BEGIN
    PRINT 'Column UserId1 does not exist';
END
GO

PRINT 'Script completed successfully';
GO

