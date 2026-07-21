namespace Contracts.DocumentsContracts;

public record ConfirmProfilePhoto(Guid UserId, Guid PhotoId, Guid? OldPhoto, bool IsPublicUser);