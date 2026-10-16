using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.PriceLists;

// Eligible: the discount is on the customer's price list and is active.
internal sealed class PriceListDiscountProvider(IPriceListReader reader) : IDiscountProvider
{
    internal const string Source = "PriceList";

    public int Priority => 100;

    public async Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<PriceListDiscountRecord> records = await reader.GetForCustomerAsync(order.CustomerId, cancellationToken);

        var discounts = new List<Discount>();

        foreach (PriceListDiscountRecord record in records.Where(r => r.IsActive).OrderBy(r => r.Id))
        {
            Result<Discount> discount = Discount.Create(record.Name, Source, record.Type, record.Value, record.Currency);

            if (discount.IsFailure)
            {
                return Result.Failure<IReadOnlyList<Discount>>(
                    PricingErrors.InvalidStoredDiscount(Source, record.Id, discount.Error));
            }

            discounts.Add(discount.Value);
        }

        return discounts;
    }
}
