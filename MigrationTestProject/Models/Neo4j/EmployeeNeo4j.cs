


namespace MigrationTestProject.Models.Neo4j
{
    public class EmployeeNeo4j
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int ExperienceLevel { get; set; } = 1;
    }
}