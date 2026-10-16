using System.Net;
using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.Customers.GetCustomerProfile;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.Fields;

public sealed class DeactivateFieldOptionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_KeepExistingSelections_ButRejectNewPicksOfTheOption()
    {
        // Arrange: one customer picked "Email" before it is deactivated, another has no value yet.
        int tenantId = await this.CreateTenantAsync();
        int customerWithEmail = await this.CreateCustomerAsync(tenantId);
        int otherCustomer = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        int email = await this.AddOptionAsync(fieldId, "Email", sortOrder: 0);
        int phone = await this.AddOptionAsync(fieldId, "Phone", sortOrder: 1);
        await this.SetOptionsAsync(customerWithEmail, fieldId, [email], rowVersion: null);

        // Act
        using HttpResponseMessage response = await HttpClient.PutAsync(
            $"fields/{fieldId}/options/{email}/deactivate", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        ProfileFieldResponse field = (await this.GetProfileAsync(customerWithEmail)).Fields.Single();
        field.SelectedOptions.Should().Equal(new OptionResponse(email, "Email"));
        field.AvailableOptions.Should().Equal(new OptionResponse(phone, "Phone"));

        Result<SetCustomerFieldValueResponse> newPick =
            await this.SetOptionsAsync(otherCustomer, fieldId, [email], rowVersion: null);
        newPick.Error.Should().Be(CustomerFieldValueErrors.InvalidOption);
    }
}
