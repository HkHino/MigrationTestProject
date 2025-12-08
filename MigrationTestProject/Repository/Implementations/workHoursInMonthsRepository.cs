using MigrationTestProject.Models.Neo4j;
using MigrationTestProject.Repositories.Neo4j;
using Neo4j.Driver;

namespace MigrationTestProject.Repository.Implementations
{
    public class WorkHoursInMonthsRepository : IWorkHoursInMonthsRepository
    {
        private readonly IDriver _driver;
        public WorkHoursInMonthsRepository(IDriver driver)
        {
            _driver = driver;
        }
        public async Task CreateWorkHoursInMonthsAsync(WorkHoursInMonthsNeo4j w)
        {
            var query = @"
                MERGE (w:WorkHoursInMonths { id: $WorkHoursInMonthId })
                SET w.bicycleNumber = $EmployeeId,
                    w.inOperate = $PayrollYear,
                    w.payrollMonth = $PayrollMonth,
                    w.periodStart = $PeriodStart,
                    w.periodEnd = $PeriodEnd,
                    w.totalHours = $TotalHours,
                    w.hasSubstituted = $HasSubstituted
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, w);
        }

    }
}
