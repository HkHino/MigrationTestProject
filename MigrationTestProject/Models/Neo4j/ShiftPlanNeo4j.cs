

namespace MigrationTestProject.Models.Neo4j
{
    public class ShiftPlanNeo4j
    {
        public string ShiftPlanId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // The shifts JSON will be migrated separately.
    }
}
