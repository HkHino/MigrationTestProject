
namespace MigrationTestProject.Models.Neo4j
{
    public class UsersNeo4j
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public UserRole Role { get; set; }
        public string Hash { get; set; }
        public string EmployeeId { get; set; }
    }
}
