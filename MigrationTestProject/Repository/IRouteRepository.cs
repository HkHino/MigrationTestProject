using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IRouteRepository
    {
        Task CreateRouteAsync(RouteNeo4j route);
    }
}
