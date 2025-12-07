using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MigrationTestProject.Models.MongoDB;

[BsonIgnoreExtraElements]
public class AuditLogDocument
{
    // MongoDB internal primary key
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? Id { get; set; }

    // Keep MySQL audit id as business id
    [BsonElement("auditId")]
    public int AuditId { get; set; }

    [BsonElement("tableName")]
    public string TableName { get; set; } = null!;

    // Typically the original table's primary key (string is correct)
    [BsonElement("recordId")]
    public string RecordId { get; set; } = null!;

    [BsonElement("action")]
    public string Action { get; set; } = null!; // "INSERT", "UPDATE", "DELETE"

    [BsonElement("changedAt")]
    public DateTime ChangedAt { get; set; }

    [BsonElement("changedBy")]
    [BsonIgnoreIfNull]
    public string? ChangedBy { get; set; }

    // Stored as raw JSON strings
    [BsonElement("oldData")]
    [BsonIgnoreIfNull]
    public string? OldData { get; set; }

    [BsonElement("newData")]
    [BsonIgnoreIfNull]
    public string? NewData { get; set; }
}