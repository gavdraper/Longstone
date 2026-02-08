using Longstone.Domain.Funds;
using MediatR;

namespace Longstone.Application.Funds.Commands.ChangeFundStatus;

public sealed record ChangeFundStatusCommand(
    Guid FundId,
    FundStatus NewStatus) : IRequest;
