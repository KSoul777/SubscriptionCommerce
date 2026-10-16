using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions;
using static SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions.Fields;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.FieldValues;

public class CustomerFieldValueTests
{
    private const int CustomerId = 12;

    [Fact]
    public void Change_Should_RecordTheFirstValueWithoutAnOldValue()
    {
        var value = CustomerFieldValue.Create(CustomerId, Text());

        Result<bool> result = value.Change(Text(), "Gold", [], "maria", UtcNow);

        result.Value.Should().BeTrue();
        value.TextValue.Should().Be("Gold");
        CustomerFieldValueHistory entry = value.History.Single();
        entry.OldValue.Should().BeNull();
        entry.NewValue.Text.Should().Be("Gold");
        entry.ChangedBy.Should().Be("maria");
        entry.ChangedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void Change_Should_ReturnFalseAndAddNoHistory_WhenTheValueIsTheSame()
    {
        CustomFieldDefinition field = Text();
        CustomerFieldValue value = SavedValue(field, "Gold");

        Result<bool> result = value.Change(field, "  Gold ", [], "maria", UtcNow.AddMinutes(1));

        result.Value.Should().BeFalse();
        value.History.Should().HaveCount(1);
        value.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void Change_Should_RecordOldAndNewValue_WhenTheValueChanges()
    {
        CustomFieldDefinition field = Text();
        CustomerFieldValue value = SavedValue(field, "Gold");

        value.Change(field, "Platinum", [], "maria", UtcNow.AddMinutes(1));

        CustomerFieldValueHistory entry = value.History.Last();
        entry.OldValue!.Text.Should().Be("Gold");
        entry.NewValue.Text.Should().Be("Platinum");
        entry.ChangedAtUtc.Should().Be(UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Change_Should_Fail_WhenATextFieldGetsOptions()
    {
        var value = CustomerFieldValue.Create(CustomerId, Text());

        Result<bool> result = value.Change(Text(), null, [10], "maria", UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.OptionsNotAllowed);
    }

    [Fact]
    public void Change_Should_Fail_WhenADropdownGetsText()
    {
        CustomFieldDefinition field = Dropdown("Email");
        var value = CustomerFieldValue.Create(CustomerId, field);

        Result<bool> result = value.Change(field, "Email", [], "maria", UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.TextNotAllowed);
    }

    [Fact]
    public void Change_Should_Fail_WhenASingleSelectDropdownGetsTwoOptions()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        var value = CustomerFieldValue.Create(CustomerId, field);

        Result<bool> result = value.Change(field, null, [10, 11], "maria", UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.MultipleSelectionNotAllowed);
    }

    [Fact]
    public void Change_Should_StoreOptionIdsAndLabelsInSortOrder_WhenMultiSelect()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone", "Post");
        var value = CustomerFieldValue.Create(CustomerId, field);

        Result<bool> result = value.Change(field, null, [12, 10], "maria", UtcNow);

        result.Value.Should().BeTrue();
        value.SelectedOptions.Select(o => o.OptionId).Should().BeEquivalentTo([10, 12]);
        value.History.Single().NewValue.Options.Should().Equal(new SnapshotOption(10, "Email"), new SnapshotOption(12, "Post"));
    }

    [Fact]
    public void Change_Should_TreatTheSameOptionsInAnotherOrderAsUnchanged()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone");
        var value = CustomerFieldValue.Create(CustomerId, field);
        value.Change(field, null, [10, 11], "maria", UtcNow);

        Result<bool> result = value.Change(field, null, [11, 10, 11], "maria", UtcNow);

        result.Value.Should().BeFalse();
    }

    [Fact]
    public void Change_Should_Fail_WhenARequiredFieldIsCleared()
    {
        CustomFieldDefinition field = Text(isRequired: true);
        CustomerFieldValue value = SavedValue(field, "Gold");

        Result<bool> result = value.Change(field, "   ", [], "maria", UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.ValueRequired);
    }

    [Fact]
    public void Change_Should_Fail_WhenTheOptionBelongsToAnotherField()
    {
        CustomFieldDefinition field = Dropdown("Email");
        var value = CustomerFieldValue.Create(CustomerId, field);

        Result<bool> result = value.Change(field, null, [99], "maria", UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.InvalidOption);
    }

    [Fact]
    public void Change_Should_Fail_WhenANewlyPickedOptionIsInactive()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        field.DeactivateOption(field.Option("Phone").Id);
        var value = CustomerFieldValue.Create(CustomerId, field);

        Result<bool> result = value.Change(field, null, [11], "maria", UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.InvalidOption);
    }

    [Fact]
    public void Change_Should_KeepASelectedOption_AfterItWasDeactivated()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone", "Post");
        var value = CustomerFieldValue.Create(CustomerId, field);
        value.Change(field, null, [10, 11], "maria", UtcNow);
        field.DeactivateOption(field.Option("Email").Id);

        Result<bool> result = value.Change(field, null, [10, 12], "maria", UtcNow.AddMinutes(1));

        result.Value.Should().BeTrue();
        value.SelectedOptions.Select(o => o.OptionId).Should().BeEquivalentTo([10, 12]);
    }

    [Fact]
    public void Change_Should_Fail_WhenTheFieldIsInactive()
    {
        CustomFieldDefinition field = Text();
        field.Deactivate();
        var value = CustomerFieldValue.Create(CustomerId, field);

        Result<bool> result = value.Change(field, "Gold", [], "maria", UtcNow);

        result.Error.Should().Be(CustomFieldErrors.Inactive);
    }

    [Fact]
    public void Change_Should_KeepTheLabelInHistoryAsItWas_WhenTheOptionIsRenamedLater()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        var value = CustomerFieldValue.Create(CustomerId, field);
        value.Change(field, null, [10], "maria", UtcNow);

        field.RenameOption(10, "E-mail");

        value.History.Single().NewValue.Options.Should().Equal(new SnapshotOption(10, "Email"));
    }

    // A value as it is after its first save: the database has assigned an id.
    private static CustomerFieldValue SavedValue(CustomFieldDefinition field, string text)
    {
        var value = CustomerFieldValue.Create(CustomerId, field);
        value.Change(field, text, [], "maria", UtcNow);

        return value.Set(v => v.Id, 100);
    }
}
