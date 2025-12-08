using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;



namespace MigrationTestProject.Repository.Implementations
{
    public class RouteRepository : IRouteRepository
    {
        private readonly IDriver _driver;
        public RouteRepository(IDriver driver)
        {
            _driver = driver;
        }
        public async Task CreateRouteAsync(RouteNeo4j r)
        {
            var query = @"
                MERGE (r:Routes { id: $Id })
                SET r.bicycleNumber = $RouteNumber
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, r);
        }
    }
}

