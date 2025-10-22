namespace Syrx.SqlServer.Tests.Performance.Models
{
    public class PerformanceTestEntity
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
        public byte[]? DataBlob { get; set; }
        public string? JsonData { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
    }

    public class ConcurrentTestEntity
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int OperationCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class SimpleKeyValue
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime Updated { get; set; }
    }

    public class BulkInsertResult
    {
        public int InsertedCount { get; set; }
        public int DurationMs { get; set; }
    }

    public class BatchUpdateResult
    {
        public int UpdatedCount { get; set; }
        public int DurationMs { get; set; }
    }

    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}