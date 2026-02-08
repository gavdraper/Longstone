namespace Longstone.Domain.Instruments;

public sealed record FixedIncomeDetails
{
    public decimal CouponRate { get; }
    public DateTime MaturityDate { get; }
    public CouponFrequency CouponFrequency { get; }
    public DayCountConvention DayCountConvention { get; }
    public DateTime LastCouponDate { get; }
    public decimal FaceValue { get; }

    public DateTime NextCouponDate => LastCouponDate.AddMonths(12 / (int)CouponFrequency);

    private FixedIncomeDetails(
        decimal couponRate,
        DateTime maturityDate,
        CouponFrequency couponFrequency,
        DayCountConvention dayCountConvention,
        DateTime lastCouponDate,
        decimal faceValue)
    {
        CouponRate = couponRate;
        MaturityDate = maturityDate;
        CouponFrequency = couponFrequency;
        DayCountConvention = dayCountConvention;
        LastCouponDate = lastCouponDate;
        FaceValue = faceValue;
    }

    public static FixedIncomeDetails Create(
        decimal couponRate,
        DateTime maturityDate,
        CouponFrequency couponFrequency,
        DayCountConvention dayCountConvention,
        DateTime lastCouponDate,
        decimal faceValue)
    {
        if (couponRate < 0m || couponRate > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(couponRate), "Coupon rate must be between 0 and 1.");
        }

        if (faceValue <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(faceValue), "Face value must be greater than zero.");
        }

        if (maturityDate <= lastCouponDate)
        {
            throw new ArgumentException("Maturity date must be after the last coupon date.", nameof(maturityDate));
        }

        return new FixedIncomeDetails(couponRate, maturityDate, couponFrequency, dayCountConvention, lastCouponDate, faceValue);
    }
}
