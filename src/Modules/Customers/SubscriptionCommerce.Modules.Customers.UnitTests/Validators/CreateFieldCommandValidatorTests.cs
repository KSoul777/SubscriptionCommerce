using FluentValidation.TestHelper;
using SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Validators;

public class CreateFieldCommandValidatorTests
{
    private static readonly CreateFieldCommand Valid = new(
        TenantId: 1,
        Name: "Industry",
        Type: FieldType.Dropdown,
        AllowMultipleSelection: false,
        IsRequired: false,
        SortOrder: 0);

    private readonly CreateFieldCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_ForAValidCommand()
    {
        _validator.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_WhenTheTypeIsUnknown()
    {
        _validator.TestValidate(Valid with { Type = (FieldType)99 })
            .ShouldHaveValidationErrorFor(c => c.Type);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Should_Fail_WhenTheNameIsEmpty(string name)
    {
        _validator.TestValidate(Valid with { Name = name })
            .ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_Should_Fail_WhenTheNameIsLongerThanTheColumn()
    {
        _validator.TestValidate(Valid with { Name = new string('x', CustomFieldDefinition.NameMaxLength + 1) })
            .ShouldHaveValidationErrorFor(c => c.Name);
    }
}
