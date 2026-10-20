using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Domain.PriceLists;

namespace SubscriptionCommerce.Modules.Pricing.Application.PriceLists;

internal sealed class PriceListDiscountProvider(IPriceListReader reader) : IDiscountProvider
{
    internal const string Source = "PriceList";

    public int Priority => 100;

    public async Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<PriceListDiscount> priceListDiscounts =
            await reader.GetActiveForCustomerAsync(order.CustomerId, cancellationToken);

        var discounts = new List<Discount>();

        foreach (PriceListDiscount item in priceListDiscounts.Where(d => d.IsEligibleFor(order.CustomerId)).OrderBy(d => d.Id))
        {
            Result<Discount> discount = Discount.Create(item.Name, Source, item.Type, item.Value, item.Currency);

            if (discount.IsFailure)
            {
                return Result.Failure<IReadOnlyList<Discount>>(
                    PricingErrors.InvalidStoredDiscount(Source, item.Id, discount.Error));
            }

            discounts.Add(discount.Value);
        }

        return discounts;
    }
}
