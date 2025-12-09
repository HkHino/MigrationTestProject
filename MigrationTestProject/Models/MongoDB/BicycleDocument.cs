using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MigrationTestProject.Models.MongoDB;

[BsonIgnoreExtraElements]
public class BicycleDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? MondoId { get; set; }

    [BsonElement("bicycleId")]
    public int Id { get; set; }

    [BsonElement("bicycleNumber")]
    public int BicycleNumber { get; set; }

    [BsonElement("inOperate")]
    public bool InOperate { get; set; } = false;
}