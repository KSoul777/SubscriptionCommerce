using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts;

// One implementation per discount source (price list, promotion, coupon, ...).
// The source decides its priority and how eligibility is looked up; the discount type decides the arithmetic.
// Implementations are registered automatically, so adding a source is adding one class.
public interface IDiscountProvider
{
    // Lower runs first. Gaps (100, 200, 300) leave room to insert a new source between existing ones.
    int Priority { get; }

    // Discounts in the order this source wants them applied. A failure fails the whole calculation.
    Task<Result<IReadOnlyList<Discount>>> GetEligibleDiscountsAsync(Order order, CancellationToken cancellationToken);
}
