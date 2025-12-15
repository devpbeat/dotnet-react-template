using MediatR;

namespace Backend.Application.Commands;

// Example MediatR Command
public record CreateSubscriptionCommand(
    Guid UserId,
    string Plan,
    string PaymentMethodId
) : IRequest<CreateSubscriptionResult>;

public record CreateSubscriptionResult(
    Guid SubscriptionId,
    string Status,
    DateTime CreatedAt
);
