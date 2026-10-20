using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

public sealed class FixedAmountDiscount : Discount
{
    private FixedAmountDiscount(string name, string source, Money amount)
        : base(name, source)
    {
        Amount = amount;
    }

    public Money Amount { get; }

    public override DiscountType Type => DiscountType.FixedAmount;

    public override string Rule => Amount.ToString();

    public override bool AppliesTo(Currency currency) => Amount.Currency == currency;

    internal static Result<Discount> Create(string name, string source, Money amount)
    {
        if (amount.Amount <= 0m)
        {
            return Result.Failure<Discount>(DiscountErrors.AmountNotPositive);
        }

        return new FixedAmountDiscount(name, source, amount);
    }

    internal override Result<Money> CalculateDeduction(Money runningPrice)
    {
        if (Amount.Currency != runningPrice.Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch);
        }

        return Money.Create(Math.Min(Amount.Amount, runningPrice.Amount), runningPrice.Currency).Value.Round();
    }
}
