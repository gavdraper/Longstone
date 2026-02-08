using FluentAssertions;
using Longstone.Application.Funds.Queries;
using Longstone.Application.Funds.Queries.GetFunds;
using Longstone.Domain.Funds;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Longstone.Application.Tests.Funds;

public class GetFundsHandlerTests
{
    private readonly IFundRepository _fundRepository = Substitute.For<IFundRepository>();
    private readonly GetFundsHandler _handler;

    public GetFundsHandlerTests()
    {
        _handler = new GetFundsHandler(_fundRepository);
    }

    private static Fund CreateFund(string name = "Test Fund", FundStatus status = FundStatus.Active)
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero));
        return Fund.Create(
            name: name,
            lei: "549300ABCDEF123456XY",
            isin: "GB00B1234567",
            fundType: FundType.OEIC,
            baseCurrency: "GBP",
            benchmarkIndex: "FTSE All-Share",
            inceptionDate: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            timeProvider: timeProvider);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedResults()
    {
        var funds = new List<Fund> { CreateFund("Fund A"), CreateFund("Fund B") };
        _fundRepository.SearchAsync(
                Arg.Is<FundSearchCriteria>(c => c.Page == 1 && c.PageSize == 20),
                Arg.Any<CancellationToken>())
            .Returns((funds.AsReadOnly() as IReadOnlyList<Fund>, 2));

        var query = new GetFundsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_PassesFilterToRepository()
    {
        var funds = new List<Fund> { CreateFund("Active Fund") };
        _fundRepository.SearchAsync(
                Arg.Is<FundSearchCriteria>(c => c.StatusFilter == FundStatus.Active),
                Arg.Any<CancellationToken>())
            .Returns((funds.AsReadOnly() as IReadOnlyList<Fund>, 1));

        var query = new GetFundsQuery(StatusFilter: FundStatus.Active);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(FundStatus.Active);
        await _fundRepository.Received(1).SearchAsync(
            Arg.Is<FundSearchCriteria>(c => c.StatusFilter == FundStatus.Active),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithManagerFilter_PassesFilterToRepository()
    {
        var managerId = Guid.NewGuid();
        var funds = new List<Fund> { CreateFund("Manager Fund") };
        _fundRepository.SearchAsync(
                Arg.Is<FundSearchCriteria>(c => c.ManagerFilter == managerId),
                Arg.Any<CancellationToken>())
            .Returns((funds.AsReadOnly() as IReadOnlyList<Fund>, 1));

        var query = new GetFundsQuery(ManagerFilter: managerId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        await _fundRepository.Received(1).SearchAsync(
            Arg.Is<FundSearchCriteria>(c => c.ManagerFilter == managerId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearchTerm_PassesTermToRepository()
    {
        var funds = new List<Fund> { CreateFund("UK Growth Fund") };
        _fundRepository.SearchAsync(
                Arg.Is<FundSearchCriteria>(c => c.SearchTerm == "UK Growth"),
                Arg.Any<CancellationToken>())
            .Returns((funds.AsReadOnly() as IReadOnlyList<Fund>, 1));

        var query = new GetFundsQuery(SearchTerm: "UK Growth");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("UK Growth Fund");
    }

    [Fact]
    public async Task Handle_WithCustomPagination_PassesPaginationToRepository()
    {
        var funds = new List<Fund> { CreateFund("Page 2 Fund") };
        _fundRepository.SearchAsync(
                Arg.Is<FundSearchCriteria>(c => c.Page == 2 && c.PageSize == 10),
                Arg.Any<CancellationToken>())
            .Returns((funds.AsReadOnly() as IReadOnlyList<Fund>, 15));

        var query = new GetFundsQuery(Page: 2, PageSize: 10);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyPaginatedList()
    {
        _fundRepository.SearchAsync(
                Arg.Any<FundSearchCriteria>(),
                Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Fund>() as IReadOnlyList<Fund>, 0));

        var query = new GetFundsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MapsFundDtoCorrectly()
    {
        var fund = CreateFund("Mapped Fund");
        _fundRepository.SearchAsync(
                Arg.Any<FundSearchCriteria>(),
                Arg.Any<CancellationToken>())
            .Returns((new List<Fund> { fund }.AsReadOnly() as IReadOnlyList<Fund>, 1));

        var query = new GetFundsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var dto = result.Items[0];
        dto.Id.Should().Be(fund.Id);
        dto.Name.Should().Be("Mapped Fund");
        dto.Lei.Should().Be(fund.Lei);
        dto.Isin.Should().Be(fund.Isin);
        dto.FundType.Should().Be(fund.FundType);
        dto.BaseCurrency.Should().Be(fund.BaseCurrency);
        dto.BenchmarkIndex.Should().Be(fund.BenchmarkIndex);
        dto.Status.Should().Be(FundStatus.Active);
        dto.Managers.Should().BeEmpty();
    }
}
