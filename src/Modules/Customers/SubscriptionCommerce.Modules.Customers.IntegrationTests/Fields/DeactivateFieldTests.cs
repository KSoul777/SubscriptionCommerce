using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Fields;

public sealed class DeactivateFieldTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_RemoveTheFieldFromProfiles_AndKeepItsHistoryReadable()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Tier");
        await this.SetTextAsync(tenantId, customerId, fieldId, "Gold", rowVersion: null);

        using HttpResponseMessage response = await HttpClient.PutAsync($"tenants/{tenantId}/fields/{fieldId}/deactivate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await this.GetProfileAsync(tenantId, customerId)).Fields.Should().BeEmpty();
        (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Select(h => h.NewValue.Text).Should().Equal("Gold");
    }

    [Fact]
    public async Task Should_Return404_AndKeepTheFieldActive_WhenCalledThroughAnotherTenant()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Tier");
        int otherTenantId = await this.CreateTenantAsync();

        using HttpResponseMessage response = await HttpClient.PutAsync(
            $"tenants/{otherTenantId}/fields/{fieldId}/deactivate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await this.GetProfileAsync(tenantId, customerId)).Fields.Select(f => f.FieldId).Should().Equal(fieldId);
    }

    [Fact]
    public async Task Should_RejectValueChanges_AfterTheFieldIsDeactivated()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Tier");
        Result<SetCustomerFieldValueResponse> gold = await this.SetTextAsync(tenantId, customerId, fieldId, "Gold", rowVersion: null);
        using HttpResponseMessage deactivate = await HttpClient.PutAsync($"tenants/{tenantId}/fields/{fieldId}/deactivate", content: null);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"tenants/{tenantId}/customers/{customerId}/fields/{fieldId}/value",
            new { textValue = "Platinum", rowVersion = gold.Value.RowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(CustomFieldErrors.Inactive.Code);
    }
}
