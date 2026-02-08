using Longstone.Domain.Instruments;
using MediatR;

namespace Longstone.Application.Instruments.Queries.GetInstrumentById;

public sealed class GetInstrumentByIdHandler(
    IInstrumentRepository instrumentRepository) : IRequestHandler<GetInstrumentByIdQuery, InstrumentDto?>
{
    public async Task<InstrumentDto?> Handle(GetInstrumentByIdQuery request, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.GetByIdAsync(request.Id, cancellationToken);

        return instrument is null ? null : InstrumentMapping.ToDto(instrument);
    }
}
