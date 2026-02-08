using Longstone.Domain.Common;

namespace Longstone.Domain.Funds;

public class Fund : IAuditable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Lei { get; private set; } = string.Empty;
    public string Isin { get; private set; } = string.Empty;
    public FundType FundType { get; private set; }
    public string BaseCurrency { get; private set; } = string.Empty;
    public string? BenchmarkIndex { get; private set; }
    public DateTime InceptionDate { get; private set; }
    public FundStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<FundManager> _assignedManagers = [];
    public IReadOnlyCollection<FundManager> AssignedManagers => _assignedManagers.AsReadOnly();

    private Fund() { }

    public static Fund Create(
        string name,
        string lei,
        string isin,
        FundType fundType,
        string baseCurrency,
        string? benchmarkIndex,
        DateTime inceptionDate,
        TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(lei);
        ArgumentException.ThrowIfNullOrWhiteSpace(isin);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseCurrency);
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (!Enum.IsDefined(fundType))
            throw new ArgumentOutOfRangeException(nameof(fundType));

        var now = timeProvider.GetUtcNow().UtcDateTime;

        return new Fund
        {
            Id = Guid.NewGuid(),
            Name = name,
            Lei = lei,
            Isin = isin,
            FundType = fundType,
            BaseCurrency = baseCurrency,
            BenchmarkIndex = benchmarkIndex,
            InceptionDate = inceptionDate,
            Status = FundStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Suspend(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (Status == FundStatus.Closed)
        {
            throw new InvalidOperationException("Cannot suspend a closed fund.");
        }

        Status = FundStatus.Suspended;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Close(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        Status = FundStatus.Closed;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Reactivate(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (Status == FundStatus.Closed)
        {
            throw new InvalidOperationException("Cannot reactivate a closed fund.");
        }

        Status = FundStatus.Active;
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void AssignManager(Guid userId, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (_assignedManagers.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException("User is already assigned as a manager of this fund.");
        }

        _assignedManagers.Add(FundManager.Create(Id, userId));
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public void RemoveManager(Guid userId, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        var manager = _assignedManagers.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException("User is not assigned as a manager of this fund.");

        _assignedManagers.Remove(manager);
        UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }
}
