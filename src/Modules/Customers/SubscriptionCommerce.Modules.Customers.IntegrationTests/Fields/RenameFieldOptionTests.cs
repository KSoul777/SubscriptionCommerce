using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Fields;

public sealed class RenameFieldOptionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_KeepTheOldLabelInHistory_WhileTheProfileShowsTheNewOne()
    {
        // Arrange: a customer picked "Email" before the option is renamed.
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        int email = await this.AddOptionAsync(fieldId, "Email", sortOrder: 0);
        await this.SetOptionsAsync(customerId, fieldId, [email], rowVersion: null);

        // Act
        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"fields/{fieldId}/options/{email}", new { label = "E-mail" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await this.GetHistoryAsync(customerId, fieldId)).Single().NewValue.Options
            .Should().Equal(new SnapshotOption(email, "Email"));

        ProfileFieldResponse field = (await this.GetProfileAsync(customerId)).Fields.Single();
        field.SelectedOptions.Should().Equal(new OptionResponse(email, "E-mail"));
    }

    [Fact]
    public async Task Should_Return409_WhenAnotherOptionHasTheLabel()
    {
        int tenantId = await this.CreateTenantAsync();
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        await this.AddOptionAsync(fieldId, "Email", sortOrder: 0);
        int phone = await this.AddOptionAsync(fieldId, "Phone", sortOrder: 1);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"fields/{fieldId}/options/{phone}", new { label = "email" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_Return404_WhenTheOptionBelongsToAnotherField()
    {
        int tenantId = await this.CreateTenantAsync();
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        int otherFieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Industry");
        int otherFieldsOption = await this.AddOptionAsync(otherFieldId, "Education", sortOrder: 0);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"fields/{fieldId}/options/{otherFieldsOption}", new { label = "Retail" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
