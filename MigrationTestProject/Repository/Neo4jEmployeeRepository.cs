using MigrationTestProject.Models.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository;
public class Neo4jEmployeeRepository
{
    private readonly IDriver _driver;

    public Neo4jEmployeeRepository(IDriver driver)
    {
        _driver = driver;
    }

    public async Task CreateEmployeeAsync(EmployeeNeo4j emp)
    {
        var query = @"
            CREATE (e:Employee {
                EmployeeId: $EmployeeId,
                FirstName: $FirstName,
                LastName: $LastName,
                Address: $Address,
                Phone: $Phone,
                Email: $Email,
                ExperienceLevel: $ExperienceLevel
            })";

        var session = _driver.AsyncSession();
        try
        {
            await session.RunAsync(query, emp);
        }
        finally
        {
            await session.CloseAsync();
        }
    }
}

