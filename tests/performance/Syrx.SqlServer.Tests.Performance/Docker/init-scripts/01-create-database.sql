-- Create database for Syrx SQL Server performance tests
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SyrxPerformance')
BEGIN
    CREATE DATABASE [SyrxPerformance];
    PRINT 'SyrxPerformance database created successfully.';
END
ELSE
BEGIN
    PRINT 'SyrxPerformance database already exists.';
END