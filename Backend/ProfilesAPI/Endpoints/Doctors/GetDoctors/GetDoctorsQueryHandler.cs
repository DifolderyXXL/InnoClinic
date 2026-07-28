using System.Diagnostics;
using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
using ProfilesAPI.Data;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public record GetDoctorsFilters(
    Status? Status = null, 
    string[]? OfficeIds = null, 
    long[]? SpecializationIds = null,
    string? FullName = null);

public record GetDoctorsQuery(PaginationParameters Pagination, GetDoctorsFilters? Filters = null) : IQuery<GetDoctorsResponse>;

public record GetDoctorsResponse(List<DoctorDto> Items, int Page, int PageSize, int Total);

public class GetDoctorsQueryHandler(ProfilesDbContext context, IPhotoUrlFactory photoUrlFactory) : IQueryHandler<GetDoctorsQuery, GetDoctorsResponse>
{
    public async Task<Result<GetDoctorsResponse>> Handle(GetDoctorsQuery query, CancellationToken ct)
    {
        var total = await context.Doctors.CountAsync(ct);
        var doctorsQuery = context.Doctors
            .OrderBy(x => x.Id)
            .Pagination(query.Pagination);

        doctorsQuery = ApplyFilters(doctorsQuery, query.Filters);
        
        var doctors = await doctorsQuery
            .ProjectToType<DoctorDto>()
            .ToListAsync(ct);

        foreach (var doctor in doctors)
        {
            if (doctor.AccountPhotoId.HasValue)
            {
                doctor.PhotoUrl = photoUrlFactory.GenerateDoctorPhotoUrl(doctor.AccountId, doctor.AccountPhotoId.Value);
            }
        }
        
        return new GetDoctorsResponse(
            doctors,
            query.Pagination.Page,
            query.Pagination.PageSize,
            total
        );
    }

    private IQueryable<Doctor> ApplyFilters(IQueryable<Doctor> query, GetDoctorsFilters? filters)
    {
        if (filters == null) return query;

        if (!string.IsNullOrWhiteSpace(filters.FullName))
        {
            var nameWords = filters.FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in nameWords)
            {
                var currentWord = word; 

                query = query.Where(d => 
                    d.Account.FirstName.Contains(currentWord) ||
                    d.Account.LastName.Contains(currentWord) ||
                    (d.Account.MiddleName != null && d.Account.MiddleName.Contains(currentWord))
                );
            }
        }

        if (filters.OfficeIds is { Length: > 0 })
        {
            query = query.Where(d => filters.OfficeIds.AsEnumerable().Contains(d.OfficeId));
        }
        
        if (filters.SpecializationIds is { Length: > 0 })
        {
            query = query.Where(d => filters.SpecializationIds.AsEnumerable().Contains(d.SpecializationId));
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(d => d.Status == filters.Status.Value);
        }

        return query;
    }
}