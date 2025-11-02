-- Seed data for Syrx SQL Server integration tests

USE [Syrx];
GO

-- Clear existing data
TRUNCATE TABLE [dbo].[poco];
TRUNCATE TABLE [dbo].[identity_test];
TRUNCATE TABLE [dbo].[bulk_insert];
TRUNCATE TABLE [dbo].[distributed_transaction];
GO

-- Populate poco table with test data (150 entries as per DatabaseBuilder.Populate())
DECLARE @i INT = 1;
DECLARE @today DATETIME = CAST(GETDATE() AS DATE);

WHILE @i < 151
BEGIN
    INSERT INTO [dbo].[poco] ([name], [value], [modified])
    VALUES (
        CONCAT('entry ', @i),
        @i * 10,
        @today
    );
    
    SET @i = @i + 1;
END;
GO

PRINT 'All databases seeded successfully.';