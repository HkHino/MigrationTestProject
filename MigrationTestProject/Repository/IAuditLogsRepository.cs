using MigrationTestProject.Models.Neo4j;


namespace MigrationTestProject.Repository
{
    public interface IAuditLogRepository
    {

        Task CreateAuditLogsAsync(AuditLogsNeo4j Audi);

    }
}
