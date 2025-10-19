using Microsoft.Extensions.DependencyInjection;
using Syrx.SqlServer.Tests.Performance.Repositories;
using Xunit;

namespace Syrx.SqlServer.Tests.Performance.Tests
{
    public class SimpleConnectionTest : IClassFixture<PerformanceTestFixture>
    {
        private readonly PerformanceTestFixture _fixture;
        private readonly IServiceProvider _serviceProvider;

        public SimpleConnectionTest(PerformanceTestFixture fixture)
        {
            _fixture = fixture;
            _serviceProvider = PerformanceTestHelper.CreateServiceProvider(_fixture.ConnectionString);
        }

        [Fact]
        public async Task DatabaseConnection_ShouldWork()
        {
            // Act
            var stats = await _fixture.GetDatabaseStatsAsync();
            
            // Assert
            Assert.True(stats.PerformanceTestCount > 0, "Should have performance test records");
        }
    }
}