using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OfficesApi.Models;

public class Office
{
    [BsonId]
    public ObjectId Id { get; set; }
    public Guid? PhotoId { get; set; }

    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    public string? OfficeNumber { get; set; }

    public string RegistryPhoneNumber { get; set; }
    public bool IsActive { get; set; }
}

