using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;
using OfficesApi.Models;
using OfficesApi.Services;

namespace OfficesApi.Endpoints.UpdateOffice;


public record UpdateOfficeCommand(string OfficeId, Guid? PhotoId, string? City, string? Street, string? HouseNumber, string? OfficeNumber, string? RegistryPhoneNumber, bool? IsActive) : ICommand;

public class UpdateOfficeCommandHandler(OfficesDbContext officesRepository, IDocumentsClient documentsClient) : ICommandHandler<UpdateOfficeCommand>
{
    public async Task<Result> Handle(UpdateOfficeCommand command, CancellationToken ct)
    {
        var officeResult = await officesRepository.GetOffice(command.OfficeId, ct);
        if (officeResult.IsError)
            return officeResult;

        
        var office = officeResult.Value!;

        if (command.PhotoId != null)
        {
            var oldPhoto = officeResult.Value!.PhotoId;
            office.PhotoId = command.PhotoId;

            await documentsClient.ConfirmOfficePhotoAsync(office.Id.ToString(), command.PhotoId.Value, oldPhoto, ct);
        }
        if (command.City != null) office.City = command.City;
        if (command.Street != null) office.Street = command.Street;
        if (command.HouseNumber != null) office.HouseNumber = command.HouseNumber;
        if (command.RegistryPhoneNumber != null) office.RegistryPhoneNumber = command.RegistryPhoneNumber;
        if (command.OfficeNumber != null) office.OfficeNumber = command.OfficeNumber;
        if (command.IsActive != null) office.IsActive = command.IsActive.Value;

        var result = await officesRepository.UpdateOffice(office, ct);

        return result;
    }
}
