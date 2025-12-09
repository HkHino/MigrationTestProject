using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repository
{
    public interface IWorkHoursInMonthsRepository
    {
        Task CreateWorkHoursInMonthsAsync(WorkHoursInMonthsNeo4j WorkHoursInMonth);
    }
}
