using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Customers;

public sealed class CustomerProfileTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ListEveryFieldOfTheTenant_WithThisCustomersValues()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int otherCustomerId = await this.CreateCustomerAsync(tenantId);
        int tier = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Tier");
        int notes = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Notes");
        await this.CreateFieldAsync(await this.CreateTenantAsync(), FieldType.Text, name: "Other tenant's field");
        await this.SetTextAsync(customerId, tier, "Gold", rowVersion: null);
        await this.SetTextAsync(otherCustomerId, tier, "Silver", rowVersion: null);

        CustomerProfileResponse? profile = await HttpClient.GetFromJsonAsync<CustomerProfileResponse>($"customers/{customerId}/profile");

        profile!.Fields.Select(f => f.FieldId).Should().BeEquivalentTo([tier, notes]);
        ProfileFieldResponse tierField = profile.Fields.Single(f => f.FieldId == tier);
        tierField.TextValue.Should().Be("Gold");
        tierField.RowVersion.Should().NotBeNullOrEmpty();
        profile.Fields.Single(f => f.FieldId == notes).TextValue.Should().BeNull();
    }

    [Fact]
    public async Task Should_Return404_WhenTheCustomerDoesNotExist()
    {
        using HttpResponseMessage response = await HttpClient.GetAsync("customers/999999/profile");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
