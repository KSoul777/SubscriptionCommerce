using FluentValidation.TestHelper;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Validators;

public class SetCustomerFieldValueCommandValidatorTests
{
    private static readonly SetCustomerFieldValueCommand Valid = new(
        TenantId: 1,
        CustomerId: 1,
        FieldId: 2,
        TextValue: "Gold",
        SelectedOptionIds: [],
        RowVersion: Convert.ToBase64String(new byte[8]));

    private readonly SetCustomerFieldValueCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_ForAValidCommand()
    {
        _validator.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_WhenAnIdIsNotPositive()
    {
        TestValidationResult<SetCustomerFieldValueCommand> result =
            _validator.TestValidate(Valid with { TenantId = 0, CustomerId = -1, FieldId = 0 });

        result.ShouldHaveValidationErrorFor(c => c.TenantId);
        result.ShouldHaveValidationErrorFor(c => c.CustomerId);
        result.ShouldHaveValidationErrorFor(c => c.FieldId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_Should_Pass_WithoutARowVersion_ForAFirstValue(string? rowVersion)
    {
        _validator.TestValidate(Valid with { RowVersion = rowVersion })
            .ShouldNotHaveValidationErrorFor(c => c.RowVersion);
    }

    [Theory]
    [InlineData("not base64!")]
    [InlineData("AAAAAA==")]
    [InlineData("AAAAAAAAAAAA")]
    public void Validate_Should_Fail_WhenTheRowVersionIsNotEightBytesOfBase64(string rowVersion)
    {
        _validator.TestValidate(Valid with { RowVersion = rowVersion })
            .ShouldHaveValidationErrorFor(c => c.RowVersion);
    }

    [Fact]
    public void Validate_Should_Fail_WhenTheSelectedOptionsAreMissing()
    {
        _validator.TestValidate(Valid with { SelectedOptionIds = null! })
            .ShouldHaveValidationErrorFor(c => c.SelectedOptionIds);
    }
}
