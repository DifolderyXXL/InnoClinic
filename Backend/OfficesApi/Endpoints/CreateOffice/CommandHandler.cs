using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;
using OfficesApi.Models;
using OfficesApi.Services;

namespace OfficesApi.Endpoints.CreateOffice;


public record CreateOfficeCommand(Guid? PhotoId, string City, string Street, string HouseNumber, string? OfficeNumber, string RegistryPhoneNumber, bool IsActive) : ICommand;
public class CreateOfficeCommandHandler(OfficesDbContext officesRepository, IDocumentsClient documentsClient) : ICommandHandler<CreateOfficeCommand>
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
            OfficeNumber = command.OfficeNumber,

            IsActive = command.IsActive
        };

        await officesRepository.Insert(office, ct);


        if (command.PhotoId != null)
        {
            await documentsClient.ConfirmOfficePhotoAsync(office.Id.ToString(), command.PhotoId.Value, null, ct);
        }
        
        return Result.Success();
    }
}
