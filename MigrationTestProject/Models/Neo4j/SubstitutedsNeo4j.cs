
namespace MigrationTestProject.Models.Neo4j
{
    public class SubstitutionRecordNeo4j
    {
        public int SubstitutedId { get; set; }
        public int EmployeeId { get; set; }
        public bool HasSubstituted { get; set; }
    }
}
