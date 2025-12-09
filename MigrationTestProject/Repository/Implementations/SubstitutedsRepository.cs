using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class SubstitutedsRepository : ISubstitutedsRepository
    {
        private readonly IDriver _driver;

        public SubstitutedsRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task CreateSubstitutedAsync(SubstitutedsNeo4j sub)
        {
            var query = @"
                MERGE (sub:Substituteds { id: $SubstitutedId })
                SET sub.hasSubstituted = $HasSubstituted
                WITH sub

                // Connect to Employee
                MATCH (e:Employees { id: $EmployeeId })
                MERGE (sub)-[:ASSIGNED_TO_EMPLOYEE]->(e)
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, sub);
        }



    }
}
