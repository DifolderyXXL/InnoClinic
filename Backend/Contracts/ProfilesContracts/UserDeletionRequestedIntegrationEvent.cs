namespace Contracts.ProfilesContracts;

public record UserDeletionRequestedIntegrationEvent(
    Guid UserId,
    DateTime RequestedAt
);