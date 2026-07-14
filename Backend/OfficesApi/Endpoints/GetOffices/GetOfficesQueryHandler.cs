using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;

namespace OfficesApi.Endpoints.GetOffices;

public record GetOfficesQuery(PaginationParameters Pagination) : IQuery<GetOfficesResponse>;

public record GetOfficesResponse(List<OfficeDto> Offices);

public class GetOfficesQueryHandler(OfficesDbContext dbContext) : IQueryHandler<GetOfficesQuery, GetOfficesResponse>
{
    public async Task<Result<GetOfficesResponse>> Handle(GetOfficesQuery query, CancellationToken ct)
    {
        var offices = await dbContext.GetAll(query.Pagination, ct);

        var dtos = offices.Select(x => new OfficeDto(
            Id: x.Id.ToString(),
            PhotoId: x.PhotoId,
            City: x.City,
            Street: x.Street,
            HouseNumber: x.HouseNumber,
            OfficeNumber: x.OfficeNumber,
            RegistryPhoneNumber: x.RegistryPhoneNumber,
            IsActive: x.IsActive
        )).ToList();

        return new GetOfficesResponse(dtos);
    }
}