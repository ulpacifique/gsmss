-- Run this script in SQL Server Management Studio to fix the migration history
-- This will mark the InitialCreate migration as applied so EF Core won't try to create existing tables

USE CommunityFinanceDB;
GO

-- Check if migration history table exists, if not create it
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Migration history table created.';
END
ELSE
BEGIN
    PRINT 'Migration history table already exists.';
END
GO

-- Mark the InitialCreate migration as applied (if it doesn't exist)
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

-- Check if new tables (Notifications, Messages, etc.) exist
-- If they do, mark the new migration as applied too
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notifications')
BEGIN
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251213233154_AddNotificationsMessagesGroups')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES ('20251213233154_AddNotificationsMessagesGroups', '10.0.0');
        PRINT 'Migration 20251213233154_AddNotificationsMessagesGroups marked as applied (tables already exist).';
    END
    ELSE
    BEGIN
        PRINT 'Migration 20251213233154_AddNotificationsMessagesGroups already exists in history.';
    END
END
ELSE
BEGIN
    PRINT 'New tables (Notifications, Messages, etc.) do not exist yet. They will be created when you run: dotnet ef database update';
END
GO

-- Show current migration history
SELECT [MigrationId], [ProductVersion] 
FROM [__EFMigrationsHistory] 
ORDER BY [MigrationId];
GO


