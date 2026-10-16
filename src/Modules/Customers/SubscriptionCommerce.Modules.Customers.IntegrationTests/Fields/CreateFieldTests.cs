using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Fields;

public sealed class CreateFieldTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return409_WhenTheTenantAlreadyHasAFieldWithThatName()
    {
        int tenantId = await this.CreateTenantAsync();

        using HttpResponseMessage first = await HttpClient.PostAsJsonAsync(
            $"tenants/{tenantId}/fields", new { name = "Industry", type = "Dropdown" });
        using HttpResponseMessage duplicate = await HttpClient.PostAsJsonAsync(
            $"tenants/{tenantId}/fields", new { name = "industry", type = "Text" });

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_Return400_WhenATextFieldAllowsMultipleSelection()
    {
        int tenantId = await this.CreateTenantAsync();

        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            $"tenants/{tenantId}/fields", new { name = "Tier", type = "Text", allowMultipleSelection = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
