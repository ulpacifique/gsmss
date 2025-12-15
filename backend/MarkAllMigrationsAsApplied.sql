-- Run this script in SQL Server Management Studio on your CommunityFinanceDB database
-- This marks all migrations as applied since your tables already exist from backup

USE CommunityFinanceDB;
GO

-- Make sure the migrations history table exists
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Mark the InitialCreate migration as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251213140242_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251213140242_InitialCreate', '10.0.0');
    PRINT 'Migration 20251213140242_InitialCreate marked as applied successfully!';
END
ELSE
BEGIN
    PRINT 'Migration 20251213140242_InitialCreate already exists in history.';
END
GO

-- Mark the AddNotificationsMessagesGroups migration as applied (if new tables exist)
-- Check if Notifications table exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications')
BEGIN
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251213233154_AddNotificationsMessagesGroups')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES ('20251213233154_AddNotificationsMessagesGroups', '10.0.0');
        PRINT 'Migration 20251213233154_AddNotificationsMessagesGroups marked as applied successfully!';
    END
    ELSE
    BEGIN
        PRINT 'Migration 20251213233154_AddNotificationsMessagesGroups already exists in history.';
    END
END
ELSE
BEGIN
    PRINT 'Notifications table does not exist. The migration will be applied when you run dotnet ef database update.';
END
GO


