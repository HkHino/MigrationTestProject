using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MigrationTestProject.Models.MongoDB;

[BsonIgnoreExtraElements]
public class EmployeeDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("employeeId")]
    public int EmployeeId { get; set; }

    [BsonElement("firstName")]
    public string FirstName { get; set; } = null!;

    [BsonElement("lastName")]
    public string LastName { get; set; } = null!;

    [BsonElement("address")]
    public string Address { get; set; } = null!;

    [BsonElement("phone")]
    public string Phone { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("experienceLevel")]
    public int ExperienceLevel { get; set; } = 1;
}