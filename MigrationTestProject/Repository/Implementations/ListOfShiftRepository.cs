using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class ListOfShiftRepository : IListOfShiftRepository
    {
        private readonly IDriver _driver;

        public ListOfShiftRepository(IDriver driver)
        {
            _driver = driver;
        }
        public async Task CreateListOfShiftAsync(ListOfShiftNeo4j l)
        {
            var query = @"
                MERGE (l:ListOfShift { shiftId: $ShiftId })
                SET l.dateOfShift = $DateOfShift,
                    l.startTime = $StartTime,
                    l.endTime = $EndTime,
                    l.totalHours = $TotalHours
                WITH l

                // Connect to Employee
                MATCH (e:Employees { id: $EmployeeId })
                MERGE (l)-[:ASSIGNED_TO_EMPLOYEE]->(e)
                WITH l

                // Connect to Bicycle
                MATCH (b:Bicycles { id: $BicycleId })
                MERGE (l)-[:USES_BICYCLE]->(b)
                WITH l

                // Connect to Route
                MATCH (r:Routes { id: $RouteId })
                MERGE (l)-[:ON_ROUTE]->(r)
                WITH l

                // Connect to Substituted
                MATCH (sub:Substituteds { id: $SubstitutedId })
                MERGE (l)-[:HAS_SUBSTITUTED]->(sub)


            ";
            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, l);



        }
    }
}
