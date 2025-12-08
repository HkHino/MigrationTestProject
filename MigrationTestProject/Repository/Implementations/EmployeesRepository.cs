using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class EmployeesRepository : IEmployeeRepository
    {
        private readonly IDriver _driver;

        public EmployeesRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task CreateEmployeeAsync(EmployeeNeo4j e)
        {
            var query = @"
            MERGE (e:Employees { employeeId: $EmployeeId })
            SET e.firstName = $FirstName,
                e.lastName = $LastName,
                e.address = $Address,
                e.phone = $Phone,
                e.email = $Email,
                e.experienceLevel = $ExperienceLevel
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, e);
        }
    }
}
