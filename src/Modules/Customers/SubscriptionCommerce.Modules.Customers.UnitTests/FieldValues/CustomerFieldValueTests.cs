using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions;
using static SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions.Fields;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.FieldValues;

public class CustomerFieldValueTests
{
    private const int CustomerId = 12;

    private static readonly Customer Contoso = Customer.Create(1, "Contoso", UtcNow).Value.Set(c => c.Id, CustomerId);

    [Fact]
    public void Create_Should_LinkTheCustomerAndTheField_AndCopyTheirTenant()
    {
        var value = CustomerFieldValue.Create(Contoso, Text());

        value.CustomerId.Should().Be(CustomerId);
        value.FieldDefinitionId.Should().Be(FieldId);
        value.TenantId.Should().Be(1);
    }

    [Fact]
    public void Create_Should_Throw_WhenTheCustomerAndTheFieldBelongToDifferentTenants()
    {
        Customer otherTenantsCustomer = Customer.Create(2, "Fabrikam", UtcNow).Value;

        Action create = () => CustomerFieldValue.Create(otherTenantsCustomer, Text());

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Change_Should_RecordTheFirstValueWithoutAnOldValue()
    {
        var value = CustomerFieldValue.Create(Contoso, Text());

        Result<bool> result = value.Change(Text().ValueRules(), "Gold", [], Maria, UtcNow);

        result.Value.Should().BeTrue();
        value.TextValue.Should().Be("Gold");
        CustomerFieldValueHistory entry = value.PendingHistory.Single();
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

        Result<bool> result = value.Change(field.ValueRules(), "  Gold ", [], Maria, UtcNow.AddMinutes(1));

        result.Value.Should().BeFalse();
        value.PendingHistory.Should().HaveCount(1);
        value.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void Change_Should_RecordOldAndNewValue_WhenTheValueChanges()
    {
        CustomFieldDefinition field = Text();
        CustomerFieldValue value = SavedValue(field, "Gold");

        value.Change(field.ValueRules(), "Platinum", [], Maria, UtcNow.AddMinutes(1));

        CustomerFieldValueHistory entry = value.PendingHistory.Last();
        entry.OldValue!.Text.Should().Be("Gold");
        entry.NewValue.Text.Should().Be("Platinum");
        entry.ChangedAtUtc.Should().Be(UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Change_Should_Fail_WhenTheTextIsLongerThanItsColumn()
    {
        var value = CustomerFieldValue.Create(Contoso, Text());

        Result<bool> result = value.Change(Text().ValueRules(), new string('x', CustomerFieldValue.TextMaxLength + 1), [], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.TextTooLong);
        value.PendingHistory.Should().BeEmpty();
    }

    [Fact]
    public void Change_Should_AcceptTextOfExactlyTheMaximumLength()
    {
        var value = CustomerFieldValue.Create(Contoso, Text());

        Result<bool> result = value.Change(Text().ValueRules(), new string('x', CustomerFieldValue.TextMaxLength), [], Maria, UtcNow);

        result.Value.Should().BeTrue();
    }

    [Fact]
    public void Change_Should_RecordAnOldValue_ForEveryChangeAfterTheFirst()
    {
        var value = CustomerFieldValue.Create(Contoso, Text());

        value.Change(Text().ValueRules(), "Gold", [], Maria, UtcNow);
        value.Change(Text().ValueRules(), "Platinum", [], Maria, UtcNow.AddMinutes(1));

        value.PendingHistory.Select(h => h.OldValue?.Text).Should().Equal(null, "Gold");
    }

    [Fact]
    public void Change_Should_Fail_WhenATextFieldGetsOptions()
    {
        var value = CustomerFieldValue.Create(Contoso, Text());

        Result<bool> result = value.Change(Text().ValueRules(), null, [10], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.OptionsNotAllowed);
    }

    [Fact]
    public void Change_Should_Fail_WhenADropdownGetsText()
    {
        CustomFieldDefinition field = Dropdown("Email");
        var value = CustomerFieldValue.Create(Contoso, field);

        Result<bool> result = value.Change(field.ValueRules(), "Email", [], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.TextNotAllowed);
    }

    [Fact]
    public void Change_Should_Fail_WhenASingleSelectDropdownGetsTwoOptions()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        var value = CustomerFieldValue.Create(Contoso, field);

        Result<bool> result = value.Change(field.ValueRules(), null, [10, 11], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.MultipleSelectionNotAllowed);
    }

    [Fact]
    public void Change_Should_StoreOptionIdsAndLabelsInSortOrder_WhenMultiSelect()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone", "Post");
        var value = CustomerFieldValue.Create(Contoso, field);

        Result<bool> result = value.Change(field.ValueRules(), null, [12, 10], Maria, UtcNow);

        result.Value.Should().BeTrue();
        value.SelectedOptions.Select(o => o.OptionId).Should().BeEquivalentTo([10, 12]);
        value.SelectedOptions.Should().OnlyContain(o => o.FieldDefinitionId == FieldId);
        value.PendingHistory.Single().NewValue.Options.Should().Equal(new SnapshotOption(10, "Email"), new SnapshotOption(12, "Post"));
    }

    [Fact]
    public void Change_Should_TreatTheSameOptionsInAnotherOrderAsUnchanged()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone");
        var value = CustomerFieldValue.Create(Contoso, field);
        value.Change(field.ValueRules(), null, [10, 11], Maria, UtcNow);

        Result<bool> result = value.Change(field.ValueRules(), null, [11, 10, 11], Maria, UtcNow);

        result.Value.Should().BeFalse();
    }

    [Fact]
    public void Change_Should_Fail_WhenARequiredFieldIsCleared()
    {
        CustomFieldDefinition field = Text(isRequired: true);
        CustomerFieldValue value = SavedValue(field, "Gold");

        Result<bool> result = value.Change(field.ValueRules(), "   ", [], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.ValueRequired);
    }

    [Fact]
    public void Change_Should_Fail_WhenTheOptionBelongsToAnotherField()
    {
        CustomFieldDefinition field = Dropdown("Email");
        var value = CustomerFieldValue.Create(Contoso, field);

        Result<bool> result = value.Change(field.ValueRules(), null, [99], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.InvalidOption);
    }

    [Fact]
    public void Change_Should_Fail_WhenANewlyPickedOptionIsInactive()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        field.DeactivateOption(field.Option("Phone").Id);
        var value = CustomerFieldValue.Create(Contoso, field);

        Result<bool> result = value.Change(field.ValueRules(), null, [11], Maria, UtcNow);

        result.Error.Should().Be(CustomerFieldValueErrors.InvalidOption);
    }

    [Fact]
    public void Change_Should_KeepASelectedOption_AfterItWasDeactivated()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone", "Post");
        var value = CustomerFieldValue.Create(Contoso, field);
        value.Change(field.ValueRules(), null, [10, 11], Maria, UtcNow);
        field.DeactivateOption(field.Option("Email").Id);

        Result<bool> result = value.Change(field.ValueRules(), null, [10, 12], Maria, UtcNow.AddMinutes(1));

        result.Value.Should().BeTrue();
        value.SelectedOptions.Select(o => o.OptionId).Should().BeEquivalentTo([10, 12]);
    }

    [Fact]
    public void Change_Should_Fail_WhenTheFieldIsInactive()
    {
        CustomFieldDefinition field = Text();
        field.Deactivate();
        var value = CustomerFieldValue.Create(Contoso, field);

        Result<bool> result = value.Change(field.ValueRules(), "Gold", [], Maria, UtcNow);

        result.Error.Should().Be(CustomFieldErrors.Inactive);
    }

    [Fact]
    public void Change_Should_KeepTheLabelInHistoryAsItWas_WhenTheOptionIsRenamedLater()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        var value = CustomerFieldValue.Create(Contoso, field);
        value.Change(field.ValueRules(), null, [10], Maria, UtcNow);

        field.RenameOption(10, "E-mail");

        value.PendingHistory.Single().NewValue.Options.Should().Equal(new SnapshotOption(10, "Email"));
    }

    [Fact]
    public void Collections_Should_RefuseChanges_EvenWhenCastBackToACollection()
    {
        CustomFieldDefinition field = MultiSelect("Email", "Phone");
        var value = CustomerFieldValue.Create(Contoso, field);
        value.Change(field.ValueRules(), null, [10], Maria, UtcNow);

        var history = (ICollection<CustomerFieldValueHistory>)value.PendingHistory;
        var selected = (ICollection<CustomerFieldValueOption>)value.SelectedOptions;

        value.PendingHistory.Should().NotBeAssignableTo<List<CustomerFieldValueHistory>>();
        value.SelectedOptions.Should().NotBeAssignableTo<List<CustomerFieldValueOption>>();
        history.Invoking(h => h.Clear()).Should().Throw<NotSupportedException>();
        selected.Invoking(s => s.Clear()).Should().Throw<NotSupportedException>();
        value.PendingHistory.Should().ContainSingle();
        value.SelectedOptions.Should().ContainSingle();

        var snapshotOptions = (IList<SnapshotOption>)value.PendingHistory.Single().NewValue.Options;
        snapshotOptions.Invoking(o => o.Clear()).Should().Throw<NotSupportedException>();
        value.PendingHistory.Single().NewValue.Options.Should().Equal(new SnapshotOption(10, "Email"));
    }

    [Fact]
    public void Change_Should_SnapshotTheOldValueWithTheLabelsKnownAtTheTimeOfTheChange()
    {
        CustomFieldDefinition field = Dropdown("Email", "Phone");
        var value = CustomerFieldValue.Create(Contoso, field);
        value.Change(field.ValueRules(), null, [10], Maria, UtcNow);
        field.RenameOption(10, "E-mail");

        value.Change(field.ValueRules(), null, [11], Maria, UtcNow.AddMinutes(1));

        CustomerFieldValueHistory[] history = value.PendingHistory.ToArray();
        history[0].NewValue.Options.Should().Equal(new SnapshotOption(10, "Email"));
        history[1].OldValue!.Options.Should().Equal(new SnapshotOption(10, "E-mail"));
        history[1].NewValue.Options.Should().Equal(new SnapshotOption(11, "Phone"));
    }

    private static CustomerFieldValue SavedValue(CustomFieldDefinition field, string text)
    {
        var value = CustomerFieldValue.Create(Contoso, field);
        value.Change(field.ValueRules(), text, [], Maria, UtcNow);

        return value;
    }
}
