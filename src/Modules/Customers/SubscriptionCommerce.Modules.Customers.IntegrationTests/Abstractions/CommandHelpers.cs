using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Customers.CreateCustomer;
using SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;
using SubscriptionCommerce.Modules.Customers.Application.Fields.AddFieldOption;
using SubscriptionCommerce.Modules.Customers.Application.Fields.CreateField;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;
using SubscriptionCommerce.Modules.Customers.Application.Tenants.CreateTenant;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

internal static class CommandHelpers
{
    internal static async Task<int> CreateTenantAsync(this BaseIntegrationTest test) =>
        (await test.SendCommand<CreateTenantCommand, int>(new CreateTenantCommand($"Tenant {Guid.NewGuid():N}"))).Value;

    internal static async Task<int> CreateCustomerAsync(this BaseIntegrationTest test, int tenantId) =>
        (await test.SendCommand<CreateCustomerCommand, int>(new CreateCustomerCommand(tenantId, "Contoso"))).Value;

    internal static async Task<int> CreateFieldAsync(
        this BaseIntegrationTest test,
        int tenantId,
        FieldType type,
        bool allowMultipleSelection = false,
        string name = "Tier") =>
        (await test.SendCommand<CreateFieldCommand, int>(
            new CreateFieldCommand(tenantId, name, type, allowMultipleSelection, IsRequired: false, SortOrder: 0))).Value;

    internal static async Task<int> AddOptionAsync(
        this BaseIntegrationTest test,
        int tenantId,
        int fieldId,
        string label,
        int sortOrder) =>
        (await test.SendCommand<AddFieldOptionCommand, int>(
            new AddFieldOptionCommand(tenantId, fieldId, label, sortOrder))).Value;

    internal static Task<Result<SetCustomerFieldValueResponse>> SetTextAsync(
        this BaseIntegrationTest test,
        int tenantId,
        int customerId,
        int fieldId,
        string text,
        string? rowVersion) =>
        test.SendCommand<SetCustomerFieldValueCommand, SetCustomerFieldValueResponse>(
            new SetCustomerFieldValueCommand(tenantId, customerId, fieldId, text, [], rowVersion));

    internal static Task<Result<SetCustomerFieldValueResponse>> SetOptionsAsync(
        this BaseIntegrationTest test,
        int tenantId,
        int customerId,
        int fieldId,
        int[] optionIds,
        string? rowVersion) =>
        test.SendCommand<SetCustomerFieldValueCommand, SetCustomerFieldValueResponse>(
            new SetCustomerFieldValueCommand(tenantId, customerId, fieldId, TextValue: null, optionIds, rowVersion));

    internal static async Task<CustomerProfileResponse> GetProfileAsync(
        this BaseIntegrationTest test,
        int tenantId,
        int customerId) =>
        (await test.SendQuery<GetCustomerProfileQuery, CustomerProfileResponse>(
            new GetCustomerProfileQuery(tenantId, customerId))).Value;

    internal static async Task<IReadOnlyList<FieldValueHistoryResponse>> GetHistoryAsync(
        this BaseIntegrationTest test,
        int tenantId,
        int customerId,
        int fieldId)
    {
        FieldValueHistoryPage page = (await test.SendQuery<GetFieldValueHistoryQuery, FieldValueHistoryPage>(
            new GetFieldValueHistoryQuery(tenantId, customerId, fieldId, After: null, GetFieldValueHistoryQuery.MaxLimit))).Value;

        page.NextAfter.Should().BeNull();

        return page.Items;
    }
}
