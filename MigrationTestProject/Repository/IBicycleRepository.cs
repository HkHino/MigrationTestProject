using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IBicycleRepository
    {
        Task CreateBicycleAsync(BicycleNeo4j bicycle);
    }
}
