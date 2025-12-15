-- Run this script in SQL Server Management Studio on your CommunityFinanceDB database
-- This marks the InitialCreate migration as applied since your tables already exist from backup

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


