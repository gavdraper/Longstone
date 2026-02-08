using FluentAssertions;
using FluentValidation;
using Longstone.Application.Funds.Commands.CreateFund;
using Longstone.Domain.Common;
using Longstone.Domain.Funds;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Funds;

public class CreateFundHandlerTests
{
    private readonly IFundRepository _fundRepository = Substitute.For<IFundRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
    private readonly CreateFundCommandHandler _handler;

    public CreateFundHandlerTests()
    {
        _handler = new CreateFundCommandHandler(_fundRepository, _unitOfWork, _timeProvider);
    }

    private static CreateFundCommand ValidCommand() =>
        new(
            Name: "UK Growth Fund",
            Lei: "549300ABCDEF123456XY",
            Isin: "GB00B1234567",
            FundType: FundType.OEIC,
            BaseCurrency: "GBP",
            BenchmarkIndex: "FTSE All-Share",
            InceptionDate: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task Handle_WithValidData_CreatesFundAndReturnsId()
    {
        var command = ValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _fundRepository.Received(1).AddAsync(Arg.Is<Fund>(f =>
            f.Name == command.Name &&
            f.Lei == command.Lei &&
            f.Isin == command.Isin &&
            f.FundType == command.FundType &&
            f.BaseCurrency == command.BaseCurrency &&
            f.BenchmarkIndex == command.BenchmarkIndex &&
            f.Status == FundStatus.Active), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullBenchmark_CreatesFundSuccessfully()
    {
        var command = ValidCommand() with { BenchmarkIndex = null };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _fundRepository.Received(1).AddAsync(
            Arg.Is<Fund>(f => f.BenchmarkIndex == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validate_WithEmptyName_FailsValidation()
    {
        var validator = new CreateFundValidator();
        var command = ValidCommand() with { Name = "" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithEmptyBaseCurrency_FailsValidation()
    {
        var validator = new CreateFundValidator();
        var command = ValidCommand() with { BaseCurrency = "" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BaseCurrency");
    }

    [Fact]
    public async Task Validate_WithEmptyLei_FailsValidation()
    {
        var validator = new CreateFundValidator();
        var command = ValidCommand() with { Lei = "" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Lei");
    }

    [Fact]
    public async Task Validate_WithEmptyIsin_FailsValidation()
    {
        var validator = new CreateFundValidator();
        var command = ValidCommand() with { Isin = "" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Isin");
    }

    [Fact]
    public async Task Validate_WithValidCommand_PassesValidation()
    {
        var validator = new CreateFundValidator();
        var command = ValidCommand();

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithInvalidFundType_FailsValidation()
    {
        var validator = new CreateFundValidator();
        var command = ValidCommand() with { FundType = (FundType)999 };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FundType");
    }
}
