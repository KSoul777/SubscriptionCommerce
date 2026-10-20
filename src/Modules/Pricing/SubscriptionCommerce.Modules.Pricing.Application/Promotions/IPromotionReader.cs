using SubscriptionCommerce.Modules.Pricing.Domain.Promotions;

namespace SubscriptionCommerce.Modules.Pricing.Application.Promotions;

public interface IPromotionReader
{
    Task<IReadOnlyList<Promotion>> GetActiveNotEndedAsync(DateTime utcNow, CancellationToken cancellationToken);
}
