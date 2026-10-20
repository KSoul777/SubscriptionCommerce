using FluentValidation;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Pricing.Domain.Coupons;

namespace SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;

internal sealed class CalculateOrderDiscountsQueryValidator : AbstractValidator<CalculateOrderDiscountsQuery>
{
    public CalculateOrderDiscountsQueryValidator()
    {
        RuleFor(q => q.CustomerId).GreaterThan(0);
        RuleFor(q => q.Price).GreaterThanOrEqualTo(0m).PrecisionScale(18, Currency.DecimalPlaces, ignoreTrailingZeros: true);
        RuleFor(q => q.Currency).NotEmpty().Length(3);
        RuleFor(q => q.CouponCodes).NotNull();
        RuleForEach(q => q.CouponCodes).NotEmpty().MaximumLength(Coupon.CodeMaxLength);
    }
}
