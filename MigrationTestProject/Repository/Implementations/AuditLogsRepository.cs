using MigrationTestProject.Models.Neo4j;
using Neo4j.Driver;


namespace MigrationTestProject.Repository.Implementations
{
    public class AuditLogsRepository: IAuditLogRepository
    {
        private readonly IDriver _driver;

        public AuditLogsRepository(IDriver driver)
        {
            _driver = driver;
        }
        public async Task CreateAuditLogsAsync(AuditLogsNeo4j a)
        {
            var query = @"
                MERGE (a:AuditLogs { id: $AuditId })
                SET a.tableName = $TableName,
                    a.recordId = $RecordId,
                    a.action = $Action,
                    a.changedAt = $ChangedAt,
                    a.changedBy = $ChangedBy,
                    a.oldData = $OldData,
                    a.newData = $NewData 
            ";

            await using var session = _driver.AsyncSession();
            await session.RunAsync(query, a);
        }


    }
}
