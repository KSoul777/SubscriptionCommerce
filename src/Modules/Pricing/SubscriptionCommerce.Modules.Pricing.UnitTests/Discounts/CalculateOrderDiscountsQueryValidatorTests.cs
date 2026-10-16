using FluentValidation.TestHelper;
using SubscriptionCommerce.Modules.Pricing.Application.Discounts.CalculateOrderDiscounts;

namespace SubscriptionCommerce.Modules.Pricing.UnitTests.Discounts;

public class CalculateOrderDiscountsQueryValidatorTests
{
    private static readonly CalculateOrderDiscountsQuery Valid = new(CustomerId: 1, Price: 340.50m, Currency: "EUR", CouponCodes: ["WELCOME10"]);

    private readonly CalculateOrderDiscountsQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_ForAValidQuery()
    {
        _validator.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_Fail_WhenTheCustomerIdIsNotPositive(int customerId)
    {
        _validator.TestValidate(Valid with { CustomerId = customerId })
            .ShouldHaveValidationErrorFor(q => q.CustomerId);
    }

    [Fact]
    public void Validate_Should_Fail_WhenThePriceIsNegative()
    {
        _validator.TestValidate(Valid with { Price = -0.01m })
            .ShouldHaveValidationErrorFor(q => q.Price);
    }

    [Fact]
    public void Validate_Should_Fail_WhenThePriceHasMoreThanTwoDecimals()
    {
        _validator.TestValidate(Valid with { Price = 340.005m })
            .ShouldHaveValidationErrorFor(q => q.Price);
    }

    [Fact]
    public void Validate_Should_AcceptTrailingZeros_InThePrice()
    {
        _validator.TestValidate(Valid with { Price = 340.5000m })
            .ShouldNotHaveValidationErrorFor(q => q.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Validate_Should_Fail_WhenTheCurrencyIsNotThreeCharacters(string currency)
    {
        _validator.TestValidate(Valid with { Currency = currency })
            .ShouldHaveValidationErrorFor(q => q.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_WhenACouponCodeIsEmpty(string code)
    {
        _validator.TestValidate(Valid with { CouponCodes = ["WELCOME10", code] })
            .ShouldHaveValidationErrorFor("CouponCodes[1]");
    }

    [Fact]
    public void Validate_Should_Fail_WhenACouponCodeIsLongerThan50Characters()
    {
        _validator.TestValidate(Valid with { CouponCodes = [new string('X', 51)] })
            .ShouldHaveValidationErrorFor("CouponCodes[0]");
    }
}
