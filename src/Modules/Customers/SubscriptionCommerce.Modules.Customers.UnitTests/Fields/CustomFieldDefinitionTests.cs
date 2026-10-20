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
            1, "Tier", FieldType.Text, allowMultipleSelection: true, isRequired: false, 0, Abstractions.Fields.Tester, Abstractions.Fields.UtcNow);

        result.Error.Should().Be(CustomFieldErrors.MultipleSelectionRequiresDropdown);
    }

    [Fact]
    public void Options_Should_RefuseChanges_EvenWhenCastBackToACollection()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        var options = (ICollection<CustomFieldOption>)field.Options;

        field.Options.Should().NotBeAssignableTo<List<CustomFieldOption>>();
        options.Invoking(o => o.Clear()).Should().Throw<NotSupportedException>();
        field.Options.Should().ContainSingle();
    }

    [Fact]
    public void ValueRules_Should_HaveNoPublicConstructor_SoOnlyTheFieldCanCreateThem()
    {
        typeof(FieldValueRules).GetConstructors().Should().BeEmpty();
        typeof(FieldValueRules).GetProperties().Should().OnlyContain(p => p.SetMethod == null);
    }

    [Fact]
    public void ValueRules_Should_BeACopy_ThatLaterChangesToTheFieldDoNotAffect()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email", "Phone");

        FieldValueRules rules = field.ValueRules();
        field.RenameOption(10, "E-mail");
        field.DeactivateOption(11);
        field.Deactivate();

        rules.IsActive.Should().BeTrue();
        rules.Options.Should().Equal(new FieldOptionRule(10, "Email", 0, true), new FieldOptionRule(11, "Phone", 1, true));
    }

    [Fact]
    public void Create_Should_StartActiveAndRecordWhoCreatedIt()
    {
        CustomFieldDefinition field = CustomFieldDefinition.Create(
            1, "  Industry ", FieldType.Dropdown, false, false, 0, Abstractions.Fields.Maria, Abstractions.Fields.UtcNow).Value;

        field.Name.Should().Be("Industry");
        field.IsActive.Should().BeTrue();
        field.CreatedBy.Should().Be("maria");
        field.CreatedAtUtc.Should().Be(Abstractions.Fields.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenTheNameIsMissing(string? name)
    {
        Result<CustomFieldDefinition> result = CustomFieldDefinition.Create(
            1, name, FieldType.Text, false, false, 0, Abstractions.Fields.Tester, Abstractions.Fields.UtcNow);

        result.Error.Should().Be(CustomFieldErrors.NameInvalid);
    }

    [Fact]
    public void Create_Should_Fail_WhenTheNameIsLongerThanItsColumn()
    {
        Result<CustomFieldDefinition> result = CustomFieldDefinition.Create(
            1, new string('x', CustomFieldDefinition.NameMaxLength + 1), FieldType.Text, false, false, 0,
            Abstractions.Fields.Tester, Abstractions.Fields.UtcNow);

        result.Error.Should().Be(CustomFieldErrors.NameInvalid);
    }

    [Fact]
    public void Create_Should_Fail_WhenTheTypeIsUnknown()
    {
        Result<CustomFieldDefinition> result = CustomFieldDefinition.Create(
            1, "Tier", (FieldType)99, false, false, 0, Abstractions.Fields.Tester, Abstractions.Fields.UtcNow);

        result.Error.Should().Be(CustomFieldErrors.UnknownType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void AddOption_Should_Fail_WhenTheLabelIsMissing(string? label)
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        Result<CustomFieldOption> result = field.AddOption(label, 1);

        result.Error.Should().Be(CustomFieldErrors.OptionLabelInvalid);
    }

    [Fact]
    public void RenameOption_Should_Fail_WhenTheLabelIsLongerThanItsColumn()
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        Result result = field.RenameOption(10, new string('x', CustomFieldOption.LabelMaxLength + 1));

        result.Error.Should().Be(CustomFieldErrors.OptionLabelInvalid);
        field.Option("Email").Id.Should().Be(10);
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

    [Theory]
    [InlineData(true, OptionChange.Add)]
    [InlineData(true, OptionChange.Rename)]
    [InlineData(true, OptionChange.Deactivate)]
    [InlineData(false, OptionChange.Add)]
    [InlineData(false, OptionChange.Rename)]
    [InlineData(false, OptionChange.Deactivate)]
    public void OptionChanges_Should_BeAllowedOnlyWhileTheFieldIsActive(bool fieldIsActive, OptionChange change)
    {
        CustomFieldDefinition field = Abstractions.Fields.Dropdown("Email");

        if (!fieldIsActive)
        {
            field.Deactivate();
        }

        Result result = change switch
        {
            OptionChange.Add => field.AddOption("Phone", 1),
            OptionChange.Rename => field.RenameOption(10, "E-mail"),
            OptionChange.Deactivate => field.DeactivateOption(10),
            _ => throw new ArgumentOutOfRangeException(nameof(change), change, null)
        };

        if (fieldIsActive)
        {
            result.IsSuccess.Should().BeTrue();
        }
        else
        {
            result.Error.Should().Be(CustomFieldErrors.Inactive);
            field.Options.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new { Id = 10, Label = "Email", IsActive = true });
        }
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

public enum OptionChange
{
    Add,
    Rename,
    Deactivate
}
