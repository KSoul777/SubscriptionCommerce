using AwesomeAssertions;
using Microsoft.Data.SqlClient;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.FieldValues;

public sealed class DatabaseIntegrityTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_RefuseAValue_ThatJoinsACustomerToAnotherTenantsField()
    {
        int tenantA = await this.CreateTenantAsync();
        int tenantB = await this.CreateTenantAsync();
        int customerOfA = await this.CreateCustomerAsync(tenantA);
        int fieldOfA = await this.CreateFieldAsync(tenantA, FieldType.Text);
        int fieldOfB = await this.CreateFieldAsync(tenantB, FieldType.Text);

        await InsertValueAsync(tenantA, customerOfA, fieldOfA);

        SqlException claimingTheCustomersTenant = await InsertValueShouldFailAsync(tenantA, customerOfA, fieldOfB);
        SqlException claimingTheFieldsTenant = await InsertValueShouldFailAsync(tenantB, customerOfA, fieldOfB);

        claimingTheCustomersTenant.Message.Should().Contain("FK_CustomerFieldValues_CustomFieldDefinitions_FieldDefinitionId_TenantId");
        claimingTheFieldsTenant.Message.Should().Contain("FK_CustomerFieldValues_Customers_CustomerId_TenantId");
    }

    [Fact]
    public async Task Should_RefuseASelectedOption_OfAnotherField()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int contact = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, allowMultipleSelection: true, name: "Contact");
        int email = await this.AddOptionAsync(tenantId, contact, "Email", 0);
        int phone = await this.AddOptionAsync(tenantId, contact, "Phone", 1);
        int industry = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Industry");
        int education = await this.AddOptionAsync(tenantId, industry, "Education", 0);
        (await this.SetOptionsAsync(tenantId, customerId, contact, [email], rowVersion: null)).IsSuccess.Should().BeTrue();

        await SelectOptionAsync(customerId, contact, fieldDefinitionId: contact, phone);

        SqlException claimingTheValuesField = await SelectOptionShouldFailAsync(customerId, contact, fieldDefinitionId: contact, education);
        SqlException claimingTheOptionsField = await SelectOptionShouldFailAsync(customerId, contact, fieldDefinitionId: industry, education);

        claimingTheValuesField.Message.Should().Contain("FK_CustomerFieldValueOptions_CustomFieldOptions_OptionId_FieldDefinitionId");
        claimingTheOptionsField.Message.Should().Contain("FK_CustomerFieldValueOptions_CustomerFieldValues_CustomerFieldValueId_FieldDefinitionId");
    }

    [Fact]
    public async Task Should_RefuseAnOptionLabel_ThatDiffersOnlyInCase_WithAKeyNoWriterCanSupply()
    {
        int tenantId = await this.CreateTenantAsync();
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        await this.AddOptionAsync(tenantId, fieldId, "Email", 0);

        Func<Task> caseOnlyDuplicate = () => ExecuteSqlAsync($"""
            INSERT INTO customers.CustomFieldOptions (FieldDefinitionId, Label, SortOrder, IsActive)
            VALUES ({fieldId}, 'email', 1, 1)
            """);
        Func<Task> keyOfItsOwn = () => ExecuteSqlAsync($"""
            INSERT INTO customers.CustomFieldOptions (FieldDefinitionId, Label, NormalizedLabel, SortOrder, IsActive)
            VALUES ({fieldId}, 'email', 'NOT THE KEY', 1, 1)
            """);

        (await caseOnlyDuplicate.Should().ThrowAsync<SqlException>())
            .Which.Message.Should().Contain("IX_CustomFieldOptions_FieldDefinitionId_NormalizedLabel");
        (await keyOfItsOwn.Should().ThrowAsync<SqlException>())
            .Which.Number.Should().Be(271);
        (await ColumnCollationAsync("CustomFieldOptions", "NormalizedLabel")).Should().Be("Latin1_General_100_BIN2");
        (await ColumnCollationAsync("CustomFieldDefinitions", "NormalizedName")).Should().Be("Latin1_General_100_BIN2");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" Email")]
    [InlineData("Email ")]
    [InlineData("\tEmail")]
    [InlineData("Email\u00A0")]
    public async Task Should_RefuseALabel_ThatIsEmptyOrHasSurroundingWhiteSpace(string label)
    {
        int tenantId = await this.CreateTenantAsync();
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");

        Func<Task> insert = () => ExecuteSqlAsync($"""
            INSERT INTO customers.CustomFieldOptions (FieldDefinitionId, Label, SortOrder, IsActive)
            VALUES ({fieldId}, {label}, 0, 1)
            """);

        (await insert.Should().ThrowAsync<SqlException>())
            .Which.Message.Should().Contain("CK_CustomFieldOptions_Label_Canonical");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Industry ")]
    public async Task Should_RefuseAFieldName_ThatIsEmptyOrHasATrailingSpace(string name)
    {
        int tenantId = await this.CreateTenantAsync();

        Func<Task> insert = () => ExecuteSqlAsync($"""
            INSERT INTO customers.CustomFieldDefinitions
                (TenantId, Name, Type, AllowMultipleSelection, IsRequired, SortOrder, IsActive, CreatedAtUtc, CreatedBy)
            VALUES ({tenantId}, {name}, 'Text', 0, 0, 0, 1, SYSUTCDATETIME(), 'import-job')
            """);

        (await insert.Should().ThrowAsync<SqlException>())
            .Which.Message.Should().Contain("CK_CustomFieldDefinitions_Name_Canonical");
    }

    [Theory]
    [InlineData("Industry")]
    [InlineData("Ελλάδα")]
    [InlineData("Ωμέγα")]
    [InlineData("Ärzte")]
    [InlineData("café")]
    [InlineData("Œuvre")]
    [InlineData("Москва")]
    [InlineData("İstanbul")]
    public async Task Should_ComputeTheSameKeyAsTheDomain_ForRepresentativeNames(string name)
    {
        string databaseKey = await QuerySingleAsync<string>($"SELECT UPPER({name} COLLATE Latin1_General_100_BIN2) AS Value");

        databaseKey.Should().Be(UniqueKey.For(name));
    }

    [Fact]
    public async Task Should_RefuseAGreekLabel_ThatDiffersOnlyInCase()
    {
        int tenantId = await this.CreateTenantAsync();
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Country");
        await this.AddOptionAsync(tenantId, fieldId, "Ελλάδα", 0);

        Func<Task> insert = () => ExecuteSqlAsync($"""
            INSERT INTO customers.CustomFieldOptions (FieldDefinitionId, Label, SortOrder, IsActive)
            VALUES ({fieldId}, {"ΕΛΛΆΔΑ"}, 1, 1)
            """);

        (await insert.Should().ThrowAsync<SqlException>())
            .Which.Message.Should().Contain("IX_CustomFieldOptions_FieldDefinitionId_NormalizedLabel");
    }

    private Task InsertValueAsync(int tenantId, int customerId, int fieldId) =>
        ExecuteSqlAsync($"""
            INSERT INTO customers.CustomerFieldValues (TenantId, CustomerId, FieldDefinitionId, TextValue, UpdatedAtUtc, UpdatedBy)
            VALUES ({tenantId}, {customerId}, {fieldId}, 'Imported', SYSUTCDATETIME(), 'import-job')
            """);

    private async Task<SqlException> InsertValueShouldFailAsync(int tenantId, int customerId, int fieldId)
    {
        Func<Task> insert = () => InsertValueAsync(tenantId, customerId, fieldId);

        return (await insert.Should().ThrowAsync<SqlException>()).Which;
    }

    private Task<string> ColumnCollationAsync(string table, string column) =>
        QuerySingleAsync<string>($"""
            SELECT collation_name AS Value
            FROM sys.columns
            WHERE object_id = OBJECT_ID('customers.' + {table}) AND name = {column}
            """);

    private Task SelectOptionAsync(int customerId, int valueFieldId, int fieldDefinitionId, int optionId) =>
        ExecuteSqlAsync($"""
            INSERT INTO customers.CustomerFieldValueOptions (CustomerFieldValueId, FieldDefinitionId, OptionId)
            SELECT Id, {fieldDefinitionId}, {optionId}
            FROM customers.CustomerFieldValues
            WHERE CustomerId = {customerId} AND FieldDefinitionId = {valueFieldId}
            """);

    private async Task<SqlException> SelectOptionShouldFailAsync(int customerId, int valueFieldId, int fieldDefinitionId, int optionId)
    {
        Func<Task> insert = () => SelectOptionAsync(customerId, valueFieldId, fieldDefinitionId, optionId);

        return (await insert.Should().ThrowAsync<SqlException>()).Which;
    }
}
