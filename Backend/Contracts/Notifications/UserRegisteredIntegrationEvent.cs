namespace Contracts.Notifications;

public record UserRegisteredIntegrationEvent(
    Guid UserId, 
    string Email, 
    string ConfirmationToken
);