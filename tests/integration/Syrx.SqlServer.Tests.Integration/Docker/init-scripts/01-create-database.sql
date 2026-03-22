-- Create the Syrx test database
USE [master];

IF NOT EXISTS (SELECT * FROM [sys].[databases] WHERE [name] = 'Syrx')
BEGIN
    CREATE DATABASE [Syrx];
END