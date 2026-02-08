namespace Longstone.Domain.Funds;

public class FundManager
{
    public Guid FundId { get; private set; }
    public Guid UserId { get; private set; }

    private FundManager() { }

    internal static FundManager Create(Guid fundId, Guid userId)
    {
        return new FundManager
        {
            FundId = fundId,
            UserId = userId
        };
    }
}
