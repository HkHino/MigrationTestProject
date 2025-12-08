using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class BicyclesRepository : IBicycleRepository
    {
        private readonly IDriver _driver;

        public BicyclesRepository(IDriver driver)
        {
            _driver = driver;
        }
        public async Task CreateBicycleAsync(BicycleNeo4j b)
        {
            var query = @"
                MERGE (b:Bicycles { id: $Id })
                SET b.bicycleNumber = $BicycleNumber,
                    b.inOperate = $InOperate
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, b);
        }



    }
}
