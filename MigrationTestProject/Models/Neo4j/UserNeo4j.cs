
namespace MigrationTestProject.Models.Neo4j
{
    class UserNeo4j
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public UserRole Role { get; set; }
        public string Hash { get; set; }
    }
}
