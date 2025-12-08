using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IShiftPlanRepository
    {
        Task CreateShiftPlansAsync(ShiftPlansNeo4j Shiftplans);
    }
}
