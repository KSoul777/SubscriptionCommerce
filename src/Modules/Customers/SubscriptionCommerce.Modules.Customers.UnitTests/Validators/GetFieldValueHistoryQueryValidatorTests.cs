using FluentValidation.TestHelper;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Validators;

public class GetFieldValueHistoryQueryValidatorTests
{
    private static readonly GetFieldValueHistoryQuery Valid = new(
        TenantId: 1, CustomerId: 2, FieldId: 3, After: null, Limit: GetFieldValueHistoryQuery.DefaultLimit);

    private readonly GetFieldValueHistoryQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_ForTheFirstPage_AndForALaterOne()
    {
        _validator.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Valid with { After = 42 }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(GetFieldValueHistoryQuery.MaxLimit + 1)]
    public void Validate_Should_Fail_WhenTheLimitIsOutsideOneToTheMaximum(int limit)
    {
        _validator.TestValidate(Valid with { Limit = limit }).ShouldHaveValidationErrorFor(q => q.Limit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_Fail_WhenAfterIsNotPositive(long after)
    {
        _validator.TestValidate(Valid with { After = after }).ShouldHaveValidationErrorFor(q => q.After);
    }
}
