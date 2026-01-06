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
            // Convert enum to string
            var parameters = new
            {
                UserId = u.UserId,
                Username = u.Username,
                Hash = u.Hash,
                Role = u.Role.ToString(),  // enum as string
                EmployeeId = u.EmployeeId
            };
            var query = @"
                MERGE (u:Users { id: $UserId })
                SET u.username = $Username,
                    u.hash = $Hash,
                    u.role = $Role
                WITH u
                
                // Connect to Employee
                MATCH (e:Employees { id: $EmployeeId })
                MERGE (u)-[:ASSIGNED_TO_EMPLOYEE]->(e)
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, parameters);
        }
    }
    
}
