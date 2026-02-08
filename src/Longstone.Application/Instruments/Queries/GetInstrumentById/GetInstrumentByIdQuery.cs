using MediatR;

namespace Longstone.Application.Instruments.Queries.GetInstrumentById;

public sealed record GetInstrumentByIdQuery(Guid Id) : IRequest<InstrumentDto?>;
