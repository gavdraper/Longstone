using MediatR;

namespace Longstone.Application.Compliance.Queries.GetMandateRuleById;

public sealed record GetMandateRuleByIdQuery(Guid Id) : IRequest<MandateRuleDto?>;
