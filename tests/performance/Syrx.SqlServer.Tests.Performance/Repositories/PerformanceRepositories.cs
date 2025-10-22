using Syrx.Commanders.Databases;
using Syrx.SqlServer.Tests.Performance.Models;

namespace Syrx.SqlServer.Tests.Performance.Repositories
{
    public class PerformanceTestRepository
    {
        private readonly ICommander<PerformanceTestRepository> _commander;

        public PerformanceTestRepository(ICommander<PerformanceTestRepository> commander)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        // Basic CRUD Operations
        public async Task<IEnumerable<PerformanceTestEntity>> GetAllAsync()
            => await _commander.QueryAsync<PerformanceTestEntity>();

        public async Task<PerformanceTestEntity?> GetByIdAsync(long id)
            => (await _commander.QueryAsync<PerformanceTestEntity>(new { id })).FirstOrDefault();

        public async Task<IEnumerable<PerformanceTestEntity>> GetByCategoryAsync(int categoryId, int limit = 100)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { categoryId, limit });

        public async Task<bool> InsertAsync(PerformanceTestEntity entity)
            => await _commander.ExecuteAsync(entity);

        public async Task<bool> UpdateAsync(PerformanceTestEntity entity)
            => await _commander.ExecuteAsync(entity);

        public async Task<bool> DeleteAsync(long id)
            => await _commander.ExecuteAsync(new { id });

        // Bulk Operations
        public async Task<IEnumerable<BulkInsertResult>> BulkInsertAsync(int batchSize = 1000, int categoryId = 1)
            => await _commander.QueryAsync<BulkInsertResult>(new { batchSize, categoryId });

        public async Task<IEnumerable<BatchUpdateResult>> BatchUpdateAsync(int categoryId, decimal multiplier = 1.1m)
            => await _commander.QueryAsync<BatchUpdateResult>(new { categoryId, multiplier });

        // Pagination
        public async Task<IEnumerable<PerformanceTestEntity>> GetPaginatedAsync(int pageSize = 50, int pageNumber = 1)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { pageSize, pageNumber });

        public async Task<int> GetTotalCountAsync()
            => (await _commander.QueryAsync<int>()).FirstOrDefault();

        // Complex Queries
        public async Task<IEnumerable<PerformanceTestEntity>> SearchByNameAsync(string searchTerm, int limit = 100)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { searchTerm = $"%{searchTerm}%", limit });

        public async Task<IEnumerable<PerformanceTestEntity>> GetRecentAsync(int days = 7, int limit = 1000)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { days, limit });

        // Joins
        public async Task<IEnumerable<dynamic>> GetWithCategoriesAsync(int limit = 100)
            => await _commander.QueryAsync<dynamic>(new { limit });

        // Aggregations
        public async Task<IEnumerable<dynamic>> GetCategoryStatsAsync()
            => await _commander.QueryAsync<dynamic>();

        public async Task<IEnumerable<dynamic>> GetDailyStatsAsync(int days = 30)
            => await _commander.QueryAsync<dynamic>(new { days });
    }

    public class ConcurrentTestRepository
    {
        private readonly ICommander<ConcurrentTestRepository> _commander;

        public ConcurrentTestRepository(ICommander<ConcurrentTestRepository> commander)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        public async Task<IEnumerable<ConcurrentTestEntity>> IncrementCounterAsync(int threadId)
            => await _commander.QueryAsync<ConcurrentTestEntity>(new { threadId });

        public async Task<IEnumerable<ConcurrentTestEntity>> GetAllCountersAsync()
            => await _commander.QueryAsync<ConcurrentTestEntity>();

        public async Task<bool> ResetCountersAsync()
        {
            await _commander.ExecuteAsync<bool>();
            return true; // Always return true for reset operation
        }
    }

    public class SimpleKeyValueRepository
    {
        private readonly ICommander<SimpleKeyValueRepository> _commander;

        public SimpleKeyValueRepository(ICommander<SimpleKeyValueRepository> commander)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        public async Task<SimpleKeyValue?> GetAsync(string key)
            => (await _commander.QueryAsync<SimpleKeyValue>(new { key })).FirstOrDefault();

        public async Task<bool> SetAsync(string key, string value)
            => await _commander.ExecuteAsync(new { key, value });

        public async Task<IEnumerable<SimpleKeyValue>> GetAllAsync()
            => await _commander.QueryAsync<SimpleKeyValue>();

        public async Task<bool> DeleteAsync(string key)
            => await _commander.ExecuteAsync(new { key });

        public async Task<IEnumerable<SimpleKeyValue>> GetByPrefixAsync(string prefix)
            => await _commander.QueryAsync<SimpleKeyValue>(new { prefix = $"{prefix}%" });
    }

    public class CategoryRepository
    {
        private readonly ICommander<CategoryRepository> _commander;

        public CategoryRepository(ICommander<CategoryRepository> commander)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _commander.QueryAsync<Category>();

        public async Task<Category?> GetByIdAsync(int id)
            => (await _commander.QueryAsync<Category>(new { id })).FirstOrDefault();

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
            => await _commander.QueryAsync<Category>();

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId)
            => await _commander.QueryAsync<Category>(new { parentId });
    }

    public class DatabaseStatsRepository
    {
        private readonly ICommander<DatabaseStatsRepository> _commander;

        public DatabaseStatsRepository(ICommander<DatabaseStatsRepository> commander)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        public async Task<DatabaseStats> GetStatsAsync()
        {
            var results = await _commander.QueryAsync<DatabaseStats>();
            return results.FirstOrDefault() ?? new DatabaseStats();
        }

        public async Task CleanupTestDataAsync()
        {
            await _commander.ExecuteAsync<bool>();
        }
    }

    public class DatabaseInitializationRepository
    {
        private readonly ICommander<DatabaseInitializationRepository> _commander;

        public DatabaseInitializationRepository(ICommander<DatabaseInitializationRepository> commander)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
        }

        public async Task<bool> CheckDatabaseExistsAsync(string databaseName)
        {
            var results = await _commander.QueryAsync<int>(new { databaseName });
            return results.FirstOrDefault() > 0;
        }

        public async Task<IEnumerable<StoredProcedureInfo>> GetStoredProceduresAsync()
        {
            return await _commander.QueryAsync<StoredProcedureInfo>();
        }
    }

    public class StoredProcedureInfo
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}