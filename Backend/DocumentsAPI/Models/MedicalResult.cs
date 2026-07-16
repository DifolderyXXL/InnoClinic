using DocumentsAPI.Application;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentsAPI.Models;

public class MedicalResult : MedicalResultBody
{
    [BsonId]
    public Guid AppointmentId { get; set; }
    
    public DateTimeOffset UpdateStamp { get; set; }
    
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    
    public UserFullName DoctorName { get; set; }
    public UserFullName PatientName { get; set; }
}