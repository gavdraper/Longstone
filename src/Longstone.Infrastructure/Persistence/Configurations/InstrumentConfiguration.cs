using Longstone.Domain.Instruments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Longstone.Infrastructure.Persistence.Configurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Isin)
            .IsRequired()
            .HasMaxLength(12);

        builder.HasIndex(i => i.Isin)
            .IsUnique();

        builder.Property(i => i.Sedol)
            .IsRequired()
            .HasMaxLength(7);

        builder.HasIndex(i => i.Sedol)
            .IsUnique();

        builder.Property(i => i.Ticker)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(i => i.Ticker);

        builder.Property(i => i.Exchange)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(i => i.CountryOfListing)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Sector)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.AssetClass)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(i => i.MarketCapitalisation)
            .HasPrecision(18, 2);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(i => i.FixedIncomeDetails, fi =>
        {
            fi.Property(f => f.CouponRate)
                .HasPrecision(8, 6)
                .HasColumnName("CouponRate");

            fi.Property(f => f.MaturityDate)
                .HasColumnName("MaturityDate");

            fi.Property(f => f.CouponFrequency)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("CouponFrequency");

            fi.Property(f => f.DayCountConvention)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasColumnName("DayCountConvention");

            fi.Property(f => f.LastCouponDate)
                .HasColumnName("LastCouponDate");

            fi.Property(f => f.FaceValue)
                .HasPrecision(18, 2)
                .HasColumnName("FaceValue");
        });

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();
    }
}
