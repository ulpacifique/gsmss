-- QUICK FIX: Run this in SQL Server Management Studio
-- This will mark the InitialCreate migration as applied so EF Core stops trying to create existing tables

USE CommunityFinanceDB;
GO

-- Mark InitialCreate as applied (if not already marked)
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251213140242_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251213140242_InitialCreate', '10.0.0');
    PRINT 'SUCCESS: InitialCreate migration marked as applied!';
END
ELSE
BEGIN
    PRINT 'InitialCreate migration already exists in history.';
END
GO

-- Verify it was added
SELECT [MigrationId], [ProductVersion] FROM [__EFMigrationsHistory] ORDER BY [MigrationId];
GO


