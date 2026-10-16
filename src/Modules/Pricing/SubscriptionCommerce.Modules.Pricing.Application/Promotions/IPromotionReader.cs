using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

namespace SubscriptionCommerce.Modules.Pricing.Application.Promotions;

public sealed record PromotionRecord(
    int Id,
    string Name,
    DiscountType Type,
    decimal Value,
    string? Currency,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    bool IsActive);

public interface IPromotionReader
{
    Task<IReadOnlyList<PromotionRecord>> GetPromotionsAsync(CancellationToken cancellationToken);
}
