using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IUsersRepository
    {
        Task CreateUsersAsync(UsersNeo4j users);
    }
}
