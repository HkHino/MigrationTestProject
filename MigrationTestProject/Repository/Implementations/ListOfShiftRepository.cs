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
            ";
            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, l);


        }
    }
}
