using MediatR;

namespace Longstone.Application.Compliance.Commands.ToggleMandateRule;

public sealed record ToggleMandateRuleCommand(
    Guid Id,
    bool IsActive) : IRequest;
