using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Queries.GetMandateRulesByFund;

public sealed class GetMandateRulesByFundHandler(
    IMandateRuleRepository mandateRuleRepository) : IRequestHandler<GetMandateRulesByFundQuery, IReadOnlyList<MandateRuleDto>>
{
    public async Task<IReadOnlyList<MandateRuleDto>> Handle(GetMandateRulesByFundQuery request, CancellationToken cancellationToken)
    {
        var rules = request.ActiveOnly
            ? await mandateRuleRepository.GetActiveByFundAsync(request.FundId, cancellationToken)
            : await mandateRuleRepository.GetByFundAsync(request.FundId, cancellationToken);

        return rules.Select(MandateRuleMapping.ToDto).ToList();
    }
}
