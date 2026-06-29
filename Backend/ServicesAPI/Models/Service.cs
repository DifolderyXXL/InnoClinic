using System;

namespace ServicesAPI.Models;

public class Service
{
    public long Id { get; set; }

    public long CategoryId { get; set; }
    public virtual ServiceCategory ServiceCategory { get; set; }

    public string ServiceName { get; set; }
    public decimal Price { get; set; }

    public long SpecializationId { get; set; }
    public virtual Specialization Specialization { get; set; }

    public bool IsActive { get; set; }
}

public class ServiceCategory
{
    public long Id { get; set; }
    public string CategoryName { get; set; }
    public TimeSpan TimeSlotSize { get; set; }

    public virtual ICollection<Service> Services { get; set; }
}

public class Specialization
{
    public long Id { get; set; }
    public string SpecializationName { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Service> Services { get; set; }
}