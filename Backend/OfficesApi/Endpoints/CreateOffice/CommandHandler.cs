using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;
using OfficesApi.Models;
using OfficesApi.Services;

namespace OfficesApi.Endpoints.CreateOffice;


public record CreateOfficeCommand(Guid? PhotoId, string City, string Street, string HouseNumber, string? OfficeNumber, string RegistryPhoneNumber, bool IsActive) : ICommand<CreateOfficeResponse>;

public record CreateOfficeResponse(string OfficeId);
public class CreateOfficeCommandHandler(OfficesDbContext officesRepository, IDocumentsClient documentsClient) : ICommandHandler<CreateOfficeCommand, CreateOfficeResponse>
{
    public async Task<Result<CreateOfficeResponse>> Handle(CreateOfficeCommand command, CancellationToken ct)
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

        var result = await officesRepository.Insert(office, ct);
        if (result.IsError)
        {
            return result.Error!;
        }


        if (command.PhotoId != null)
        {
            await documentsClient.ConfirmOfficePhotoAsync(office.Id.ToString(), command.PhotoId.Value, null, ct);
        }
        
        return new CreateOfficeResponse(result.Value!);
    }
}
