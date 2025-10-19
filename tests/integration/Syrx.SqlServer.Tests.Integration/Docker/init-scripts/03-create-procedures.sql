-- Create stored procedures for Syrx SQL Server integration tests
USE [Syrx];

-- Drop procedures if they exist
DROP PROCEDURE IF EXISTS [dbo].[usp_create_table];
DROP PROCEDURE IF EXISTS [dbo].[usp_identity_tester];
DROP PROCEDURE IF EXISTS [dbo].[usp_bulk_insert];
DROP PROCEDURE IF EXISTS [dbo].[usp_bulk_insert_and_return];
DROP PROCEDURE IF EXISTS [dbo].[usp_clear_table];

-- Create table creator procedure
CREATE PROCEDURE [dbo].[usp_create_table]
(@name nvarchar(max))
AS
BEGIN
    DECLARE @template nvarchar(max)
           ,@sql nvarchar(max);

    SELECT @template
        = N'
            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[%name]'') AND type in (N''U'')) 
                BEGIN
                    DROP TABLE [dbo].[%name];
                END 

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[%name]'') AND type in (N''U'')) 
                BEGIN 
                    CREATE TABLE [dbo].[%name] 
                    (
                        [id] [int] IDENTITY(1, 1) NOT NULL,
                        [name] [varchar](50) NULL,
                        [value] [decimal](18, 2) NULL,
                        [modified] [datetime] NULL,
                        CONSTRAINT [PK_%name] PRIMARY KEY CLUSTERED ([id] ASC)
                    ); 
                END;';

    SELECT @sql = REPLACE(@template, '%name', @name);
    EXEC [sys].[sp_executesql] @sql;
END;
GO

-- Create identity tester procedure
CREATE PROCEDURE [dbo].[usp_identity_tester]
    @name varchar(50)
   ,@value decimal(18, 2)
AS
BEGIN
    INSERT INTO [identity_test]
    (
        [name]
       ,[value]
       ,[modified]
    )
    SELECT @name
          ,@value
          ,GETUTCDATE();

    SELECT SCOPE_IDENTITY();
END;
GO

-- Create bulk insert procedure
CREATE PROCEDURE [dbo].[usp_bulk_insert]
(@path varchar(max))
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @command nvarchar(max)
           ,@template nvarchar(max) = N'
                BULK INSERT [dbo].[bulk_insert] FROM ''%path'' WITH (FIELDTERMINATOR = '','', ROWTERMINATOR = ''\n'')';

    SELECT @command = REPLACE(@template, '%path', @path);
    EXEC [sys].[sp_executesql] @command;
END;
GO

-- Create bulk insert and return procedure
CREATE PROCEDURE [dbo].[usp_bulk_insert_and_return]
(@path varchar(max))
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @command nvarchar(max)
           ,@template nvarchar(max) = N'
                BULK INSERT [dbo].[bulk_insert] FROM ''%path'' WITH (FIELDTERMINATOR = '','', ROWTERMINATOR = ''\n'')';

    SELECT @command = REPLACE(@template, '%path', @path);
    EXEC [sys].[sp_executesql] @command;

    SELECT *
    FROM [dbo].[bulk_insert];
END;
GO

-- Create table clearing procedure
CREATE PROCEDURE [dbo].[usp_clear_table]
(@name nvarchar(max))
AS
BEGIN
    DECLARE @template nvarchar(max)
           ,@sql nvarchar(max);

    SELECT @template
        = N'TRUNCATE TABLE [%name];';

    SELECT @sql = REPLACE(@template, '%name', @name);
    EXEC [sys].[sp_executesql] @sql;
END;

PRINT 'All stored procedures created successfully.';