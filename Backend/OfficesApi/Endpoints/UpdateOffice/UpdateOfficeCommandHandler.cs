using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;
using OfficesApi.Models;

namespace OfficesApi.Endpoints.UpdateOffice;


public record UpdateOfficeCommand(string OfficeId, long? PhotoId, string City, string Street, string HouseNumber, string? OfficeNumber, string RegistryPhoneNumber, bool IsActive) : ICommand;

public class UpdateOfficeCommandHandler(OfficesDbContext officesRepository) : ICommandHandler<UpdateOfficeCommand>
{
    public async Task<Result> Handle(UpdateOfficeCommand command, CancellationToken ct)
    {
        var office = new Office
        {
            Id = new(command.OfficeId),
            PhotoId = command.PhotoId,
            City = command.City,
            Street = command.Street,
            HouseNumber = command.HouseNumber,
            RegistryPhoneNumber = command.RegistryPhoneNumber,
            OfficeNumber = command.OfficeNumber,

            IsActive = command.IsActive
        };

        var result = await officesRepository.UpdateOffice(office, ct);

        return result;
    }
}
