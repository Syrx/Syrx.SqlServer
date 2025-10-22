-- Create tables for Syrx SQL Server performance tests
USE [SyrxPerformance];

-- Create a large test table for bulk operations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[performance_test]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[performance_test] 
    (
        [id] [bigint] IDENTITY(1,1) NOT NULL,
        [name] [varchar](100) NOT NULL,
        [description] [varchar](500) NULL,
        [value] [decimal](18, 4) NOT NULL,
        [created_date] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [modified_date] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [category_id] [int] NOT NULL,
        [is_active] [bit] NOT NULL DEFAULT 1,
        [data_blob] [varbinary](max) NULL,
        [json_data] [nvarchar](max) NULL,
        CONSTRAINT [PK_performance_test] PRIMARY KEY CLUSTERED ([id] ASC)
    );
    
    -- Create indexes for performance testing
    CREATE NONCLUSTERED INDEX [IX_performance_test_name] ON [dbo].[performance_test] ([name]);
    CREATE NONCLUSTERED INDEX [IX_performance_test_category] ON [dbo].[performance_test] ([category_id]);
    CREATE NONCLUSTERED INDEX [IX_performance_test_created] ON [dbo].[performance_test] ([created_date]);
    CREATE NONCLUSTERED INDEX [IX_performance_test_composite] ON [dbo].[performance_test] ([category_id], [is_active], [created_date]);
END

-- Create a categories table for joins
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[categories] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NOT NULL,
        [description] [varchar](200) NULL,
        [parent_id] [int] NULL,
        CONSTRAINT [PK_categories] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_categories_parent] FOREIGN KEY ([parent_id]) REFERENCES [dbo].[categories]([id])
    );
    
    CREATE NONCLUSTERED INDEX [IX_categories_parent] ON [dbo].[categories] ([parent_id]);
END

-- Create a table for concurrent access testing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[concurrent_test]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[concurrent_test] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [thread_id] [int] NOT NULL,
        [operation_count] [int] NOT NULL DEFAULT 0,
        [last_updated] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_concurrent_test] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create a simple key-value table for basic operations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[simple_kv]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[simple_kv] 
    (
        [key] [varchar](50) NOT NULL,
        [value] [varchar](500) NOT NULL,
        [updated] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_simple_kv] PRIMARY KEY CLUSTERED ([key] ASC)
    );
END

PRINT 'All performance test tables created successfully.';