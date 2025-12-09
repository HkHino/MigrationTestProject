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
                SET w.payrollYear = $PayrollYear,
                    w.payrollMonth = $PayrollMonth,
                    w.periodStart = $PeriodStart,
                    w.periodEnd = $PeriodEnd,
                    w.totalHours = $TotalHours,
                    w.hasSubstituted = $HasSubstituted
                WITH w
                
                // Connect to Employee
                MATCH (e:Employees { id: $EmployeeId })
                MERGE (w)-[:BELONGS_TO]->(e)
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, new
            {
                WorkHoursInMonthId = w.WorkHoursInMonthId,
                PayrollYear = w.PayrollYear,
                PayrollMonth = w.PayrollMonth,
                PeriodStart = w.PeriodStart,
                PeriodEnd = w.PeriodEnd,
                TotalHours = (double)w.TotalHours, // cast to double
                HasSubstituted = w.HasSubstituted,
                EmployeeId = w.EmployeeId
            });
        }

    }
}
