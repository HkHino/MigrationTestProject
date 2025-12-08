using MigrationTestProject.Models.Neo4j;

namespace MigrationTestProject.Repositories.Neo4j
{
    public interface ISubstitutedsRepository
    {
        Task CreateSubstitutedAsync(SubstitutedsNeo4j substituteds);
    }
}
