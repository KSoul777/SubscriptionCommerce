using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.FieldValues;

public sealed class SetCustomerFieldValueTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_WriteHistoryOnlyWhenTheValueChanges()
    {
        (int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        Result<SetCustomerFieldValueResponse> first = await this.SetTextAsync(customerId, fieldId, "Gold", rowVersion: null);
        Result<SetCustomerFieldValueResponse> repeat = await this.SetTextAsync(customerId, fieldId, "Gold", first.Value.RowVersion);

        first.Value.Changed.Should().BeTrue();
        repeat.Value.Changed.Should().BeFalse();
        repeat.Value.RowVersion.Should().Be(first.Value.RowVersion);
        (await this.GetHistoryAsync(customerId, fieldId)).Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_Return409ProblemDetails_WhenTheRowVersionIsStale()
    {
        (int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();
        string url = $"customers/{customerId}/fields/{fieldId}/value";

        SetCustomerFieldValueResponse first = await PutTextAsync(url, "Gold", rowVersion: null);
        await PutTextAsync(url, "Platinum", first.RowVersion);

        // A second user still holding the first version tries to save.
        using HttpResponseMessage stale = await HttpClient.PutAsJsonAsync(url, new { textValue = "Silver", rowVersion = first.RowVersion });

        stale.StatusCode.Should().Be(HttpStatusCode.Conflict);
        stale.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        ProblemDetails? problem = await stale.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(CustomerFieldValueErrors.ConcurrencyConflict.Code);
        (await this.GetHistoryAsync(customerId, fieldId)).Select(h => h.NewValue.Text).Should().Equal("Gold", "Platinum");
    }

    [Fact]
    public async Task Should_LetOnlyOneOfTwoConcurrentFirstValuesWin()
    {
        (int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        // Both requests believe there is no value yet. Whichever loses either sees the winner's row
        // (stale row version) or hits the unique index on (CustomerId, FieldDefinitionId).
        Result<SetCustomerFieldValueResponse>[] results = await Task.WhenAll(
            this.SetTextAsync(customerId, fieldId, "Gold", rowVersion: null),
            this.SetTextAsync(customerId, fieldId, "Platinum", rowVersion: null));

        results.Should().ContainSingle(r => r.IsSuccess);
        results.Single(r => r.IsFailure).Error.Should().Be(CustomerFieldValueErrors.ConcurrencyConflict);
        (await this.GetHistoryAsync(customerId, fieldId)).Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_KeepHistoryInOrder_WithEachOldValueEqualToThePreviousNewValue()
    {
        (int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        string? rowVersion = null;
        foreach (string text in new[] { "Gold", "Platinum", "Silver" })
        {
            rowVersion = (await this.SetTextAsync(customerId, fieldId, text, rowVersion)).Value.RowVersion;
        }

        IReadOnlyList<FieldValueHistoryResponse> history = await this.GetHistoryAsync(customerId, fieldId);

        history.Select(h => h.Id).Should().BeInAscendingOrder();
        history.Select(h => h.NewValue.Text).Should().Equal("Gold", "Platinum", "Silver");
        history[0].OldValue.Should().BeNull();
        history[1].OldValue.Should().BeEquivalentTo(history[0].NewValue);
        history[2].OldValue.Should().BeEquivalentTo(history[1].NewValue);
        history.Should().OnlyContain(h => h.ChangedAtUtc.Kind == DateTimeKind.Utc);
    }

    [Fact]
    public async Task Should_StoreSelectedOptionIdsAndLabels_ForAMultiSelectDropdown()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, allowMultipleSelection: true, name: "Channels");
        int email = await this.AddOptionAsync(fieldId, "Email", sortOrder: 0);
        int phone = await this.AddOptionAsync(fieldId, "Phone", sortOrder: 1);

        Result<SetCustomerFieldValueResponse> result = await SendCommand<SetCustomerFieldValueCommand, SetCustomerFieldValueResponse>(
            new SetCustomerFieldValueCommand(customerId, fieldId, TextValue: null, SelectedOptionIds: [phone, email], RowVersion: null));

        result.IsSuccess.Should().BeTrue();
        FieldValueHistoryResponse entry = (await this.GetHistoryAsync(customerId, fieldId)).Single();
        entry.NewValue.Options.Should().BeEquivalentTo(
            [new SnapshotOption(email, "Email"), new SnapshotOption(phone, "Phone")],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Should_Return404_ForAFieldOfAnotherTenant()
    {
        (int customerId, _) = await CreateCustomerWithTextFieldAsync();
        int otherTenantId = await this.CreateTenantAsync();
        int otherTenantsField = await this.CreateFieldAsync(otherTenantId, FieldType.Text);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"customers/{customerId}/fields/{otherTenantsField}/value",
            new { textValue = "Gold" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_MoveTheRowVersionOn_WhenOnlyTheSelectedOptionsChange()
    {
        // Arrange: with a frozen clock and the same user, UpdatedAtUtc and UpdatedBy stay identical,
        // so swapping one option for another changes no column of the value row itself.
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        int email = await this.AddOptionAsync(fieldId, "Email", sortOrder: 0);
        int phone = await this.AddOptionAsync(fieldId, "Phone", sortOrder: 1);

        Clock.FrozenAt = DateTimeOffset.UtcNow;
        try
        {
            string? first = (await this.SetOptionsAsync(customerId, fieldId, [email], rowVersion: null)).Value.RowVersion;

            // Act
            Result<SetCustomerFieldValueResponse> swapped = await this.SetOptionsAsync(customerId, fieldId, [phone], first);
            Result<SetCustomerFieldValueResponse> stale = await this.SetOptionsAsync(customerId, fieldId, [email], first);

            // Assert
            swapped.Value.Changed.Should().BeTrue();
            swapped.Value.RowVersion.Should().NotBe(first);
            stale.Error.Should().Be(CustomerFieldValueErrors.ConcurrencyConflict);
        }
        finally
        {
            Clock.FrozenAt = null;
        }
    }

    [Fact]
    public async Task Should_RecordTheCallerFromTheUserHeader()
    {
        (int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        using var request = new HttpRequestMessage(HttpMethod.Put, $"customers/{customerId}/fields/{fieldId}/value")
        {
            Content = JsonContent.Create(new { textValue = "Gold" })
        };
        request.Headers.Add("X-User-Name", "maria");

        using HttpResponseMessage response = await HttpClient.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await this.GetHistoryAsync(customerId, fieldId)).Single().ChangedBy.Should().Be("maria");
    }

    private async Task<(int CustomerId, int FieldId)> CreateCustomerWithTextFieldAsync()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text);

        return (customerId, fieldId);
    }

    private async Task<SetCustomerFieldValueResponse> PutTextAsync(string url, string text, string? rowVersion)
    {
        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(url, new { textValue = text, rowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<SetCustomerFieldValueResponse>())!;
    }
}
