

namespace MigrationTestProject.Models.Neo4j
{
    public class AuditLogsNeo4j
    {
        public int AuditId { get; set; }
        public string TableName { get; set; } = null!;
        public string RecordId { get; set; } = null!;
        public string Action { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
    }
}
