-- Seed initial data for Syrx SQL Server performance tests
USE [SyrxPerformance];

-- Clear existing data
TRUNCATE TABLE [dbo].[performance_test];
DELETE FROM [dbo].[categories];
TRUNCATE TABLE [dbo].[concurrent_test];
DELETE FROM [dbo].[simple_kv];

-- Reset identity seed to ensure consistent category IDs
DBCC CHECKIDENT('[dbo].[categories]', RESEED, 0);

-- Insert categories (Electronics will be ID=1)
INSERT INTO [dbo].[categories] (name, description) VALUES
('Electronics', 'Electronic devices and components'),
('Books', 'Books and literature'),
('Clothing', 'Apparel and accessories'),
('Home & Garden', 'Home improvement and gardening supplies'),
('Sports', 'Sports equipment and accessories'),
('Automotive', 'Car parts and accessories'),
('Health', 'Health and wellness products'),
('Food', 'Food and beverage items'),
('Toys', 'Toys and games'),
('Music', 'Musical instruments and media');

-- Insert subcategories
INSERT INTO [dbo].[categories] (name, description, parent_id) VALUES
('Smartphones', 'Mobile phones and smartphones', 1),
('Laptops', 'Laptop computers', 1),
('Fiction', 'Fiction books', 2),
('Non-Fiction', 'Non-fiction books', 2),
('Mens Clothing', 'Clothing for men', 3),
('Womens Clothing', 'Clothing for women', 3);

-- Insert initial performance test data (10,000 records) using optimized bulk insert approach
-- Create a numbers table for bulk generation
WITH Numbers AS (
    SELECT 1 as n
    UNION ALL
    SELECT n + 1 FROM Numbers WHERE n < 10000
),
CategoryIds AS (
    SELECT id FROM [dbo].[categories] WHERE parent_id IS NULL
),
RandomData AS (
    SELECT 
        n,
        (SELECT TOP 1 id FROM CategoryIds ORDER BY NEWID()) as category_id,
        CAST(RAND(CHECKSUM(NEWID())) * 1000 + (n * 0.01) AS DECIMAL(18,4)) as random_value
    FROM Numbers
)
INSERT INTO [dbo].[performance_test] 
(name, description, value, category_id, json_data, data_blob)
SELECT
    CONCAT('Initial Item ', n),
    CONCAT('Seeded performance test item #', n, ' for benchmarking'),
    random_value,
    category_id,
    CONCAT('{"seed_id":', n, ',"random_value":', random_value, ',"category":', category_id, '}'),
    CAST(CONCAT('Binary data for item ', n, ' - ', REPLICATE('X', n % 100 + 1)) AS VARBINARY(MAX))
FROM RandomData
OPTION (MAXRECURSION 10000);

-- Insert some key-value pairs for simple operations
INSERT INTO [dbo].[simple_kv] ([key], [value]) VALUES
('config.timeout', '30'),
('config.batch_size', '1000'),
('config.max_connections', '100'),
('test.start_time', CONVERT(VARCHAR(30), GETUTCDATE(), 120)),
('test.environment', 'performance'),
('app.version', '1.0.0'),
('db.schema_version', '1.0'),
('cache.enabled', 'true'),
('logging.level', 'INFO'),
('performance.warmup_complete', 'false');

PRINT 'Performance test data seeded successfully.';
PRINT CONCAT('Inserted ', @@ROWCOUNT, ' initial records for performance testing.');