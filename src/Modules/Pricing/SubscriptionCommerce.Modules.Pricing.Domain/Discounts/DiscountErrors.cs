using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Domain.Discounts;

public static class DiscountErrors
{
    public static readonly Error NameRequired = Error.Problem(
        "Discounts.NameRequired",
        $"The discount name is required and must be at most {Discount.NameMaxLength} characters");

    public static readonly Error SourceRequired = Error.Problem(
        "Discounts.SourceRequired",
        "The discount source is required");

    public static readonly Error UnknownType = Error.Problem(
        "Discounts.UnknownType",
        "The discount type must be Percentage or FixedAmount");

    public static readonly Error PercentageOutOfRange = Error.Problem(
        "Discounts.PercentageOutOfRange",
        "A percentage discount must be greater than 0 and at most 100");

    public static readonly Error PercentageWithCurrency = Error.Problem(
        "Discounts.PercentageWithCurrency",
        "A percentage discount can't have a currency");

    public static readonly Error AmountNotPositive = Error.Problem(
        "Discounts.AmountNotPositive",
        "A fixed amount discount must be greater than 0");

    public static readonly Error NegativePrice = Error.Problem(
        "Discounts.NegativePrice",
        "The price to discount can't be negative");
}
