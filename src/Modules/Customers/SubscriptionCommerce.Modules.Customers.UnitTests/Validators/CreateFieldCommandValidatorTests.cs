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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_Fail_WhenTheTenantIdIsNotPositive(int tenantId)
    {
        _validator.TestValidate(Valid with { TenantId = tenantId })
            .ShouldHaveValidationErrorFor(c => c.TenantId);
    }
}
