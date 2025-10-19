-- Create tables for Syrx SQL Server integration tests
USE [Syrx];

-- Create the main poco table used in most tests
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[poco]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[poco] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_poco] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create identity_test table for identity testing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[identity_test]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[identity_test] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_identity_test] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create bulk_insert table for bulk operations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[bulk_insert]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[bulk_insert] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_bulk_insert] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create distributed_transaction table for distributed transaction tests
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[distributed_transaction]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[distributed_transaction] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_distributed_transaction] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

PRINT 'All test tables created successfully.';