using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class ShiftPlansRepository : IShiftPlanRepository
    {
        private readonly IDriver _driver;
        public ShiftPlansRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task CreateShiftPlansAsync(ShiftPlansNeo4j s)
        {
            var query = @"
                MERGE (s:Bicycles { id: $ShiftPlanId })
                SET s.name = $Name,
                    s.startDate= $StartDate,
                    s.endDate = $EndDate,
                    s.shifts = $Shifts
            ";
            // The shifts JSON will be migrated separately
            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, s);


        }


    }
}
