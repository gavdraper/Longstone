using FluentAssertions;
using Longstone.Application.Common.Exceptions;
using Longstone.Application.Funds.Commands.ChangeFundStatus;
using Longstone.Domain.Common;
using Longstone.Domain.Funds;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Funds;

public class ChangeFundStatusHandlerTests
{
    private readonly IFundRepository _fundRepository = Substitute.For<IFundRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly ChangeFundStatusCommandHandler _handler;

    public ChangeFundStatusHandlerTests()
    {
        _handler = new ChangeFundStatusCommandHandler(_fundRepository, _unitOfWork, _timeProvider);
    }

    private Fund CreateFund(FundStatus? targetStatus = null)
    {
        var fund = Fund.Create("Test Fund", "549300ABCDEF123456XY", "GB00B1234567",
            FundType.OEIC, "GBP", null,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), _timeProvider);

        if (targetStatus == FundStatus.Suspended)
            fund.Suspend(_timeProvider);
        else if (targetStatus == FundStatus.Closed)
            fund.Close(_timeProvider);

        return fund;
    }

    [Fact]
    public async Task Handle_SuspendActiveFund_SuspendsFund()
    {
        var fund = CreateFund();
        _fundRepository.GetByIdAsync(fund.Id, Arg.Any<CancellationToken>()).Returns(fund);

        await _handler.Handle(new ChangeFundStatusCommand(fund.Id, FundStatus.Suspended), CancellationToken.None);

        fund.Status.Should().Be(FundStatus.Suspended);
        _fundRepository.Received(1).Update(fund);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CloseActiveFund_ClosesFund()
    {
        var fund = CreateFund();
        _fundRepository.GetByIdAsync(fund.Id, Arg.Any<CancellationToken>()).Returns(fund);

        await _handler.Handle(new ChangeFundStatusCommand(fund.Id, FundStatus.Closed), CancellationToken.None);

        fund.Status.Should().Be(FundStatus.Closed);
    }

    [Fact]
    public async Task Handle_ReactivateSuspendedFund_ActivatesFund()
    {
        var fund = CreateFund(FundStatus.Suspended);
        _fundRepository.GetByIdAsync(fund.Id, Arg.Any<CancellationToken>()).Returns(fund);

        await _handler.Handle(new ChangeFundStatusCommand(fund.Id, FundStatus.Active), CancellationToken.None);

        fund.Status.Should().Be(FundStatus.Active);
    }

    [Fact]
    public async Task Handle_ReactivateClosedFund_ThrowsInvalidOperationException()
    {
        var fund = CreateFund(FundStatus.Closed);
        _fundRepository.GetByIdAsync(fund.Id, Arg.Any<CancellationToken>()).Returns(fund);

        var act = () => _handler.Handle(
            new ChangeFundStatusCommand(fund.Id, FundStatus.Active), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_NonExistentFund_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _fundRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Fund?)null);

        var act = () => _handler.Handle(
            new ChangeFundStatusCommand(id, FundStatus.Suspended), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Validate_WithEmptyFundId_FailsValidation()
    {
        var validator = new ChangeFundStatusValidator();
        var command = new ChangeFundStatusCommand(Guid.Empty, FundStatus.Suspended);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FundId");
    }

    [Fact]
    public async Task Validate_WithInvalidStatus_FailsValidation()
    {
        var validator = new ChangeFundStatusValidator();
        var command = new ChangeFundStatusCommand(Guid.NewGuid(), (FundStatus)999);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }

    [Fact]
    public async Task Validate_WithValidCommand_PassesValidation()
    {
        var validator = new ChangeFundStatusValidator();
        var command = new ChangeFundStatusCommand(Guid.NewGuid(), FundStatus.Suspended);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
