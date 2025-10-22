-- Create stored procedures for Syrx SQL Server performance tests
USE [SyrxPerformance]

-- Drop procedures if they exist
IF OBJECT_ID('[dbo].[usp_bulk_insert_performance]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[usp_bulk_insert_performance]
GO

IF OBJECT_ID('[dbo].[usp_get_by_category]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[usp_get_by_category]
GO

IF OBJECT_ID('[dbo].[usp_get_paginated]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[usp_get_paginated]
GO

IF OBJECT_ID('[dbo].[usp_update_batch]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[usp_update_batch]
GO

IF OBJECT_ID('[dbo].[usp_increment_counter]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[usp_increment_counter]
GO

-- Bulk insert procedure for performance testing
CREATE PROCEDURE [dbo].[usp_bulk_insert_performance]
    @batchSize INT = 1000,
    @categoryId INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @i INT = 1;
    DECLARE @batch_start DATETIME2 = GETUTCDATE();
    
    WHILE @i <= @batchSize
    BEGIN
        INSERT INTO [dbo].[performance_test] 
        (name, description, value, category_id, json_data)
        VALUES 
        (
            CONCAT('Item ', @i, ' - ', FORMAT(@batch_start, 'yyyy-MM-dd HH:mm:ss')),
            CONCAT('Performance test item #', @i, ' generated at ', @batch_start),
            RAND() * 1000,
            @categoryId,
            CONCAT('{"id":', @i, ',"batch_start":"', @batch_start, '","category":', @categoryId, '}')
        );
        
        SET @i = @i + 1;
    END;
    
    SELECT @batchSize as InsertedCount, DATEDIFF(MILLISECOND, @batch_start, GETUTCDATE()) as DurationMs;
END
GO

-- Get by category with performance metrics
CREATE PROCEDURE [dbo].[usp_get_by_category]
    @categoryId INT,
    @limit INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@limit)
        id,
        name,
        description,
        value,
        created_date,
        category_id
    FROM [dbo].[performance_test]
    WHERE category_id = @categoryId
        AND is_active = 1
    ORDER BY created_date DESC;
END
GO

-- Paginated results procedure
CREATE PROCEDURE [dbo].[usp_get_paginated]
    @pageSize INT = 50,
    @pageNumber INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @offset INT = (@pageNumber - 1) * @pageSize;
    
    SELECT 
        id,
        name,
        description,
        value,
        created_date,
        category_id
    FROM [dbo].[performance_test]
    WHERE is_active = 1
    ORDER BY id
    OFFSET @offset ROWS
    FETCH NEXT @pageSize ROWS ONLY;
    
    -- Return total count for pagination
    SELECT COUNT(*) as total_count
    FROM [dbo].[performance_test]
    WHERE is_active = 1;
END
GO

-- Batch update procedure
CREATE PROCEDURE [dbo].[usp_update_batch]
    @category_id INT,
    @multiplier DECIMAL(18,4) = 1.1
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @start_time DATETIME2 = GETUTCDATE();
    DECLARE @updated_count INT;
    
    UPDATE [dbo].[performance_test]
    SET value = value * @multiplier,
        modified_date = GETUTCDATE()
    WHERE category_id = @category_id
        AND is_active = 1;
    
    SET @updated_count = @@ROWCOUNT;
    
    SELECT 
        @updated_count as UpdatedCount,
        DATEDIFF(MILLISECOND, @start_time, GETUTCDATE()) as DurationMs;
END
GO

-- Counter increment for concurrency testing
CREATE PROCEDURE [dbo].[usp_increment_counter]
    @threadId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Try to update existing record
    UPDATE [dbo].[concurrent_test]
    SET operation_count = operation_count + 1,
        last_updated = GETUTCDATE()
    WHERE thread_id = @threadId;
    
    -- If no record exists, insert new one
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO [dbo].[concurrent_test] (thread_id, operation_count)
        VALUES (@threadId, 1);
    END;
    
    SELECT id as Id, thread_id as ThreadId, operation_count as OperationCount, last_updated as LastUpdated 
    FROM [dbo].[concurrent_test] 
    WHERE thread_id = @threadId;
END
GO