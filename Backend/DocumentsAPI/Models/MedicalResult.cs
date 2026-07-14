using MongoDB.Bson.Serialization.Attributes;

namespace DocumentsAPI.Models;

public class MedicalResult : MedicalResultBody
{
    [BsonId]
    public Guid AppointmentId { get; set; }
    
    public Guid UserId { get; set; }
}