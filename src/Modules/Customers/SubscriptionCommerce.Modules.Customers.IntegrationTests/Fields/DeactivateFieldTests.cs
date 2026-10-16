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
        // Arrange
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Tier");
        await this.SetTextAsync(customerId, fieldId, "Gold", rowVersion: null);

        // Act
        using HttpResponseMessage response = await HttpClient.PutAsync($"fields/{fieldId}/deactivate", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await this.GetProfileAsync(customerId)).Fields.Should().BeEmpty();
        (await this.GetHistoryAsync(customerId, fieldId)).Select(h => h.NewValue.Text).Should().Equal("Gold");
    }

    [Fact]
    public async Task Should_RejectValueChanges_AfterTheFieldIsDeactivated()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text, name: "Tier");
        Result<SetCustomerFieldValueResponse> gold = await this.SetTextAsync(customerId, fieldId, "Gold", rowVersion: null);
        using HttpResponseMessage deactivate = await HttpClient.PutAsync($"fields/{fieldId}/deactivate", content: null);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"customers/{customerId}/fields/{fieldId}/value",
            new { textValue = "Platinum", rowVersion = gold.Value.RowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(CustomFieldErrors.Inactive.Code);
    }
}
