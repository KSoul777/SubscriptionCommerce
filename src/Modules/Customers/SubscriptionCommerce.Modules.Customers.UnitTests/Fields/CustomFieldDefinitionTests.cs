using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Fields;

public class CustomFieldDefinitionTests
{
    [Fact]
    public void Create_Should_Fail_WhenATextFieldAllowsMultipleSelection()
    {
        Result<CustomFieldDefinition> result = CustomFieldDefinition.Create(
            1, "Tier", FieldType.Text, allowMultipleSelection: true, isRequired: false, 0, "tester", Abstractions.Fields.UtcNow);

        result.Error.Should().Be(CustomFieldErrors.MultipleSelectionRequiresDropdown);
    }

    [Fact]
    public void Create_Should_StartActiveAndRecordWhoCreatedIt()
    {
        CustomFieldDefinition field = CustomFieldDefinition.Create(
            1, "  Industry ", FieldType.Dropdown, false, false, 0, "maria", Abstractions.Fields.UtcNow).Value;

        field.Name.Should().Be("Industry");
        field.IsActive.Should().BeTrue();
        field.CreatedBy.Should().Be("maria");
        field.CreatedAtUtc.Should().Be(Abstractions.Fields.UtcNow);
    }

    [Fact]
    public void AddOption_Should_Fail_ForATextField()
    {
        CustomFieldDefinition field = Abstractions.Fields.Text();

        Result<CustomFieldOption> result = field.AddOption("Gold", 0);

        result.Error.Should().Be(CustomFieldErrors.OptionsRequireDropdown);
    }

    [Fact]
    public void AddOption_Should_Fail_WhenTheLabelExistsIgnoringCase()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        Result<CustomFieldOption> result = field.AddOption(" email ", 1);

        result.Error.Should().Be(CustomFieldErrors.DuplicateOptionLabel);
    }

    [Fact]
    public void AddOption_Should_Fail_WhenTheFieldIsInactive()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");
        field.Deactivate();

        Result<CustomFieldOption> result = field.AddOption("Phone", 1);

        result.Error.Should().Be(CustomFieldErrors.Inactive);
    }

    [Fact]
    public void RenameOption_Should_ChangeTheLabel()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email", "Phone");

        Result result = field.RenameOption(10, "  E-mail ");

        result.IsSuccess.Should().BeTrue();
        field.Option("E-mail").Id.Should().Be(10);
    }

    [Fact]
    public void RenameOption_Should_AllowChangingOnlyTheCasingOfItsOwnLabel()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("email");

        Result result = field.RenameOption(10, "Email");

        result.IsSuccess.Should().BeTrue();
        field.Options.Single().Label.Should().Be("Email");
    }

    [Fact]
    public void RenameOption_Should_Fail_WhenAnotherOptionHasTheLabelIgnoringCase()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email", "Phone");

        Result result = field.RenameOption(11, "EMAIL");

        result.Error.Should().Be(CustomFieldErrors.DuplicateOptionLabel);
        field.Option("Phone").Id.Should().Be(11);
    }

    [Fact]
    public void RenameOption_Should_Fail_WhenTheFieldHasNoSuchOption()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        Result result = field.RenameOption(99, "Phone");

        result.Error.Should().Be(CustomFieldErrors.OptionNotFound(99));
    }

    [Fact]
    public void RenameOption_Should_Fail_WhenTheFieldIsInactive()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");
        field.Deactivate();

        Result result = field.RenameOption(10, "E-mail");

        result.Error.Should().Be(CustomFieldErrors.Inactive);
    }

    [Fact]
    public void DeactivateOption_Should_KeepTheOptionButMarkItInactive()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email", "Phone");

        Result first = field.DeactivateOption(10);
        Result again = field.DeactivateOption(10);

        first.IsSuccess.Should().BeTrue();
        again.IsSuccess.Should().BeTrue();
        field.Options.Should().HaveCount(2);
        field.Option("Email").IsActive.Should().BeFalse();
        field.Option("Phone").IsActive.Should().BeTrue();
    }

    [Fact]
    public void DeactivateOption_Should_Fail_WhenTheFieldHasNoSuchOption()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        Result result = field.DeactivateOption(99);

        result.Error.Should().Be(CustomFieldErrors.OptionNotFound(99));
    }

    [Fact]
    public void Deactivate_Should_MarkTheFieldInactiveAndKeepItsOptions()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        field.Deactivate();
        field.Deactivate();

        field.IsActive.Should().BeFalse();
        field.Options.Single().IsActive.Should().BeTrue();
    }
}
