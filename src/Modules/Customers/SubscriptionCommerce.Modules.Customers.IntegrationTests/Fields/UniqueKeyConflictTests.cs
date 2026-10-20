using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionCommerce.Modules.Customers.Application.Abstractions.Data;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Domain.Users;
using SubscriptionCommerce.Modules.Customers.Infrastructure.Database;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Fields;

public sealed class UniqueKeyConflictTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Theory]
    [InlineData("Cannot insert duplicate key row in object 'customers.CustomFieldDefinitions' with unique index 'IX_CustomFieldDefinitions_TenantId_NormalizedName'.", ConflictKind.DuplicateFieldName)]
    [InlineData("Cannot insert duplicate key row in object 'customers.CustomFieldOptions' with unique index 'IX_CustomFieldOptions_FieldDefinitionId_NormalizedLabel'.", ConflictKind.DuplicateOptionLabel)]
    [InlineData("Cannot insert duplicate key row in object 'customers.CustomerFieldValues' with unique index 'IX_CustomerFieldValues_CustomerId_FieldDefinitionId'.", ConflictKind.DuplicateFieldValue)]
    [InlineData("Violation of PRIMARY KEY constraint 'PK_CustomerFieldValueOptions'. Cannot insert duplicate key in object 'customers.CustomerFieldValueOptions'.", ConflictKind.DuplicateSelectedOption)]
    public void UniqueKeyConflict_Should_NameTheConflict_ForAKnownUniqueIndex(string sqlErrorMessage, ConflictKind expected)
    {
        CustomersDbContext.UniqueKeyConflict(sqlErrorMessage).Should().Be(expected);
    }

    [Theory]
    [InlineData("Violation of UNIQUE KEY constraint 'AK_Customers_Id_TenantId'. Cannot insert duplicate key in object 'customers.Customers'.")]
    [InlineData("Cannot insert duplicate key row in object 'customers.CustomFieldOptions' with unique index 'IX_CustomFieldOptions_FieldDefinitionId_NormalizedLabel_Archived'.")]
    public void UniqueKeyConflict_Should_LeaveAnyOtherUniqueIndexUntranslated(string sqlErrorMessage)
    {
        CustomersDbContext.UniqueKeyConflict(sqlErrorMessage).Should().BeNull();
    }

    [Fact]
    public async Task Should_ReportALabelSavedByAnotherRequestFirst_AsADuplicateOptionLabel()
    {
        int tenantId = await this.CreateTenantAsync();
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        using IServiceScope first = CreateScope();
        using IServiceScope second = CreateScope();
        CustomFieldDefinition firstCopy = (await Fields(first).GetAsync(tenantId, fieldId))!;
        CustomFieldDefinition secondCopy = (await Fields(second).GetAsync(tenantId, fieldId))!;

        firstCopy.AddOption("Email", 0);
        secondCopy.AddOption("email", 1);
        await UnitOfWork(first).SaveChangesAsync();
        Func<Task> save = () => UnitOfWork(second).SaveChangesAsync();

        (await save.Should().ThrowAsync<ConcurrencyConflictException>())
            .Which.Kind.Should().Be(ConflictKind.DuplicateOptionLabel);
    }

    [Fact]
    public async Task Should_ReportTheSameOptionAddedByAnotherRequestFirst_AsAConflictTheValueHandlerMaps()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, allowMultipleSelection: true, name: "Contact");
        int email = await this.AddOptionAsync(tenantId, fieldId, "Email", 0);
        int phone = await this.AddOptionAsync(tenantId, fieldId, "Phone", 1);
        (await this.SetOptionsAsync(tenantId, customerId, fieldId, [email], rowVersion: null)).IsSuccess.Should().BeTrue();
        UserName maria = UserName.Create("maria").Value;
        using IServiceScope first = CreateScope();
        using IServiceScope second = CreateScope();
        CustomerFieldValue firstCopy = (await Values(first).GetAsync(customerId, fieldId))!;
        CustomerFieldValue secondCopy = (await Values(second).GetAsync(customerId, fieldId))!;
        FieldValueRules rules = (await Fields(first).GetAsync(tenantId, fieldId))!.ValueRules();

        firstCopy.Change(rules, null, [email, phone], maria, DateTime.UtcNow);
        secondCopy.Change(rules, null, [email, phone], maria, DateTime.UtcNow);
        await UnitOfWork(first).SaveChangesAsync();
        Func<Task> save = () => UnitOfWork(second).SaveChangesAsync();

        (await save.Should().ThrowAsync<ConcurrencyConflictException>())
            .Which.Kind.Should().BeOneOf(ConflictKind.RowVersion, ConflictKind.DuplicateSelectedOption);
    }

    private static ICustomerFieldValueRepository Values(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ICustomerFieldValueRepository>();

    private static ICustomFieldDefinitionRepository Fields(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ICustomFieldDefinitionRepository>();

    private static IUnitOfWork UnitOfWork(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
}
