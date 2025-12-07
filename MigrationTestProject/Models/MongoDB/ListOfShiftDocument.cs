using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MigrationTestProject.Models.MongoDB;

[BsonIgnoreExtraElements]
public class ListOfShiftDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]        // <- important
    public string? Id { get; set; }
    
    [BsonElement("shiftId")]
    public int ShiftId { get; set; }

    [BsonElement("dateOfShift")]
    public DateTime DateOfShift { get; set; }

    [BsonElement("employeeId")]
    public int EmployeeId { get; set; }

    [BsonElement("bicycleId")]
    public int BicycleId { get; set; }

    [BsonElement("substitutedId")]
    public int SubstitutedId { get; set; }

    [BsonElement("routeId")]
    public int RouteId { get; set; }

    [BsonElement("startTime")]
    public string? StartTime { get; set; }

    [BsonElement("endTime")]
    public string? EndTime { get; set; }

    [BsonElement("totalHours")]
    public decimal? TotalHours { get; set; }
    
    [BsonElement("employeeRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? EmployeeRefId { get; set; }
    
    [BsonElement("bicycleRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? BicycleRefId { get; set; }

    [BsonElement("substitutedRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? SubstitutedRefId { get; set; }
    
    [BsonElement("routeRefId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? RouteRefId { get; set; }
}