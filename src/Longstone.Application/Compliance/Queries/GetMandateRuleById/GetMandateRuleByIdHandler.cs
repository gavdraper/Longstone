using Longstone.Domain.Compliance;
using MediatR;

namespace Longstone.Application.Compliance.Queries.GetMandateRuleById;

public sealed class GetMandateRuleByIdHandler(
    IMandateRuleRepository mandateRuleRepository) : IRequestHandler<GetMandateRuleByIdQuery, MandateRuleDto?>
{
    public async Task<MandateRuleDto?> Handle(GetMandateRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await mandateRuleRepository.GetByIdAsync(request.Id, cancellationToken);
        return rule is null ? null : MandateRuleMapping.ToDto(rule);
    }
}
