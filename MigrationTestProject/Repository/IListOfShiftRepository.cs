using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface IListOfShiftRepository
    {
        Task CreateListOfShiftAsync(ListOfShiftNeo4j ListOfShift);
    }
}
