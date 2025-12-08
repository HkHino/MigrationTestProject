using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class UsersRepository : IUsersRepository
    {
        private readonly IDriver _driver;

        public UsersRepository(IDriver driver)
        {
            _driver = driver;
        }
        public async Task CreateUsersAsync(UsersNeo4j u)
        {
            var query = @"
                MERGE (u:Users { id: $UserID })
                SET u.username = $Username,
                    u.hash = $hash,
                    u.role = $role,
                    u.employeeId =$EmployeeId
            ";
            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, u);
        }
    }
    
}
