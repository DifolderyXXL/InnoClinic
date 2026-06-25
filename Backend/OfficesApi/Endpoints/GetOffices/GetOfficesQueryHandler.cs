using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;

namespace OfficesApi.Endpoints.GetOffices;

public record GetOfficesQuery() : IQuery<GetOfficesResponse>;

public record GetOfficesResponse(List<OfficeDto> offices);

public class GetOfficesQueryHandler(OfficesDbContext dbContext) : IQueryHandler<GetOfficesQuery, GetOfficesResponse>
{
    public async Task<Result<GetOfficesResponse>> Handle(GetOfficesQuery query, CancellationToken ct)
    {
        var offices = await dbContext.GetAll(ct);

        var dtos = offices.Select(x => new OfficeDto(
            Id: x.Id.ToString(),
            PhotoId: x.PhotoId,
            City: x.City,
            Street: x.Street,
            HouseNumber: x.HouseNumber,
            RegistryPhoneNumber: x.RegistryPhoneNumber,
            IsActive: x.IsActive
        )).ToList();

        return new GetOfficesResponse(dtos);
    }
}