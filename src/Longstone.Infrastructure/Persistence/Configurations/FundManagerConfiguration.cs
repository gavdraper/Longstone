using Longstone.Domain.Auth;
using Longstone.Domain.Funds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class FundManagerConfiguration : IEntityTypeConfiguration<FundManager>
{
    public void Configure(EntityTypeBuilder<FundManager> builder)
    {
        builder.HasKey(fm => new { fm.FundId, fm.UserId });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(fm => fm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
