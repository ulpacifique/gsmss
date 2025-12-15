-- Use this script if you have restored a database backup and the tables already exist
-- This will mark the migrations as applied so EF Core won't try to create them again

-- Check if __EFMigrationsHistory table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Insert the InitialCreate migration if it doesn't exist
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251213140242_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251213140242_InitialCreate', '10.0.0');
    PRINT 'Migration 20251213140242_InitialCreate marked as applied';
END
ELSE
BEGIN
    PRINT 'Migration 20251213140242_InitialCreate already exists in history';
END

