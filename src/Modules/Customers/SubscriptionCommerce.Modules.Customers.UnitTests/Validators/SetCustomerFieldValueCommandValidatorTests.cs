using FluentValidation.TestHelper;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Validators;

public class SetCustomerFieldValueCommandValidatorTests
{
    private static readonly SetCustomerFieldValueCommand Valid = new(
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
    [InlineData("AAAAAA==")]         // 4 bytes
    [InlineData("AAAAAAAAAAAA")]     // 9 bytes
    public void Validate_Should_Fail_WhenTheRowVersionIsNotEightBytesOfBase64(string rowVersion)
    {
        _validator.TestValidate(Valid with { RowVersion = rowVersion })
            .ShouldHaveValidationErrorFor(c => c.RowVersion);
    }

    [Fact]
    public void Validate_Should_Fail_WhenTheTextIsLongerThanTheColumn()
    {
        _validator.TestValidate(Valid with { TextValue = new string('x', CustomerFieldValue.TextMaxLength + 1) })
            .ShouldHaveValidationErrorFor(c => c.TextValue);
    }

    [Fact]
    public void Validate_Should_AcceptTextOfExactlyTheMaximumLength()
    {
        _validator.TestValidate(Valid with { TextValue = new string('x', CustomerFieldValue.TextMaxLength) })
            .ShouldNotHaveValidationErrorFor(c => c.TextValue);
    }

    [Fact]
    public void Validate_Should_Fail_WhenTheSelectedOptionsAreMissing()
    {
        _validator.TestValidate(Valid with { SelectedOptionIds = null! })
            .ShouldHaveValidationErrorFor(c => c.SelectedOptionIds);
    }
}
