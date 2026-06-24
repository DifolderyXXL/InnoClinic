using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;
using OfficesApi.Models;

namespace OfficesApi.Endpoints.CreateOffice;


public record CreateOfficeCommand(long? PhotoId, string City, string Street, string HouseNumber, string RegistryPhoneNumber, string IsActive) : ICommand;
public class CreateOfficeCommandHandler(OfficesDbContext officesRepository) : ICommandHandler<CreateOfficeCommand>
{
    public async Task<Result> Handle(CreateOfficeCommand command, CancellationToken ct)
    {
        var office = new Office
        {
            PhotoId = command.PhotoId,
            City = command.City,
            Street = command.Street,
            HouseNumber = command.HouseNumber,
            RegistryPhoneNumber = command.RegistryPhoneNumber,

            IsActive = bool.TryParse(command.IsActive, out var isActive) && isActive
        };

        await officesRepository.Insert(office, ct);

        return Result.Success();
    }
}

