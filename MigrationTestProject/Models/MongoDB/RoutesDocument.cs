using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MigrationTestProject.Models.MongoDB;

[BsonIgnoreExtraElements]
public class RouteDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? Id { get; set; }

    // Keep your MySQL numeric ID if needed
    [BsonElement("routeId")]
    public int RouteId { get; set; }

    [BsonElement("routeNumber")]
    public int RouteNumber { get; set; }
}