using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IEmployeeRepository
    {
        Task CreateEmployeeAsync(EmployeeNeo4j employee);
    }
}
