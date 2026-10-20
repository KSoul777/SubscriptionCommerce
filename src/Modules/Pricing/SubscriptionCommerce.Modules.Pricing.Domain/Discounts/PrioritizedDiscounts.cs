namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

public sealed record PrioritizedDiscounts(int Priority, IReadOnlyList<Discount> Discounts);
