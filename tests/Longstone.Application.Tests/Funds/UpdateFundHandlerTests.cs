using FluentAssertions;
using Longstone.Application.Common.Exceptions;
using Longstone.Application.Funds.Commands.UpdateFund;
using Longstone.Domain.Common;
using Longstone.Domain.Funds;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Funds;

public class UpdateFundHandlerTests
{
    private readonly IFundRepository _fundRepository = Substitute.For<IFundRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly UpdateFundCommandHandler _handler;

    public UpdateFundHandlerTests()
    {
        _handler = new UpdateFundCommandHandler(_fundRepository, _unitOfWork, _timeProvider);
    }

    private Fund CreateFund()
    {
        return Fund.Create("Original", "549300ABCDEF123456XY", "GB00B1234567",
            FundType.OEIC, "GBP", "FTSE 100",
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), _timeProvider);
    }

    [Fact]
    public async Task Handle_WithExistingFund_UpdatesAllPropertiesAndSaves()
    {
        var fund = CreateFund();
        _fundRepository.GetByIdAsync(fund.Id, Arg.Any<CancellationToken>()).Returns(fund);
        _timeProvider.Advance(TimeSpan.FromMinutes(5));

        var command = new UpdateFundCommand(
            Id: fund.Id,
            Name: "Updated Name",
            Lei: "549300NEWLEI999999AB",
            Isin: "GB00B9999999",
            FundType: FundType.UnitTrust,
            BaseCurrency: "USD",
            BenchmarkIndex: "S&P 500",
            InceptionDate: new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        await _handler.Handle(command, CancellationToken.None);

        fund.Name.Should().Be("Updated Name");
        fund.Lei.Should().Be("549300NEWLEI999999AB");
        fund.Isin.Should().Be("GB00B9999999");
        fund.FundType.Should().Be(FundType.UnitTrust);
        fund.BaseCurrency.Should().Be("USD");
        fund.BenchmarkIndex.Should().Be("S&P 500");
        fund.InceptionDate.Should().Be(new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        fund.UpdatedAt.Should().Be(_timeProvider.GetUtcNow().UtcDateTime);
        fund.UpdatedAt.Should().BeAfter(fund.CreatedAt);

        _fundRepository.Received(1).Update(fund);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentFund_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _fundRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Fund?)null);

        var command = new UpdateFundCommand(id, "Name", "549300ABCDEF123456XY", "GB00B1234567",
            FundType.OEIC, "GBP", null,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Validate_WithEmptyName_FailsValidation()
    {
        var validator = new UpdateFundValidator();
        var command = new UpdateFundCommand(Guid.NewGuid(), "", "549300ABCDEF123456XY", "GB00B1234567",
            FundType.OEIC, "GBP", null,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithEmptyId_FailsValidation()
    {
        var validator = new UpdateFundValidator();
        var command = new UpdateFundCommand(Guid.Empty, "Name", "549300ABCDEF123456XY", "GB00B1234567",
            FundType.OEIC, "GBP", null,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
