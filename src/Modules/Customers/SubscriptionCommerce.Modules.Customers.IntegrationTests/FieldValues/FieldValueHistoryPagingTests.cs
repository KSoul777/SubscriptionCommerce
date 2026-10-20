using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.FieldValues;

public sealed class FieldValueHistoryPagingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReadHistoryAPageAtATime_WithoutSkippingOrRepeatingEntries()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();
        string? rowVersion = await WriteAsync(tenantId, customerId, fieldId, null, "A", "B", "C", "D", "E");
        string url = $"tenants/{tenantId}/customers/{customerId}/fields/{fieldId}/history";

        FieldValueHistoryPage first = await GetPageAsync($"{url}?limit=2");
        await WriteAsync(tenantId, customerId, fieldId, rowVersion, "F");
        FieldValueHistoryPage second = await GetPageAsync($"{url}?limit=2&after={first.NextAfter}");
        FieldValueHistoryPage third = await GetPageAsync($"{url}?limit=2&after={second.NextAfter}");

        Texts(first).Should().Equal("A", "B");
        Texts(second).Should().Equal("C", "D");
        Texts(third).Should().Equal("E", "F");
        third.NextAfter.Should().BeNull();
    }

    [Fact]
    public async Task Should_ReturnUpToTheDefaultLimit_WhenNoPageIsAskedFor()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();
        await WriteAsync(tenantId, customerId, fieldId, null, "A", "B");

        FieldValueHistoryPage page = await GetPageAsync($"tenants/{tenantId}/customers/{customerId}/fields/{fieldId}/history");

        Texts(page).Should().Equal("A", "B");
        page.NextAfter.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return400_WhenTheLimitIsAboveTheMaximum()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        using HttpResponseMessage response = await HttpClient.GetAsync(
            $"tenants/{tenantId}/customers/{customerId}/fields/{fieldId}/history?limit={GetFieldValueHistoryQuery.MaxLimit + 1}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static IEnumerable<string?> Texts(FieldValueHistoryPage page) => page.Items.Select(h => h.NewValue.Text);

    private async Task<FieldValueHistoryPage> GetPageAsync(string url)
    {
        using HttpResponseMessage response = await HttpClient.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<FieldValueHistoryPage>())!;
    }

    private async Task<string?> WriteAsync(int tenantId, int customerId, int fieldId, string? rowVersion, params string[] texts)
    {
        foreach (string text in texts)
        {
            rowVersion = (await this.SetTextAsync(tenantId, customerId, fieldId, text, rowVersion)).Value.RowVersion;
        }

        return rowVersion;
    }

    private async Task<(int TenantId, int CustomerId, int FieldId)> CreateCustomerWithTextFieldAsync()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text);

        return (tenantId, customerId, fieldId);
    }
}
