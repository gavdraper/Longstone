using MediatR;

namespace Longstone.Application.Compliance.Queries.GetMandateRulesByFund;

public sealed record GetMandateRulesByFundQuery(
    Guid FundId,
    bool ActiveOnly = false) : IRequest<IReadOnlyList<MandateRuleDto>>;
