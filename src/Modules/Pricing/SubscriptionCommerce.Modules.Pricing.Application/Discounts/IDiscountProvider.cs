using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts;

public interface IDiscountProvider
{
    int Priority { get; }

    Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken);
}
