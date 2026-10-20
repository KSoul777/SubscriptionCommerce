using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.GetFieldValueHistory;
using SubscriptionCommerce.Modules.Customers.Application.FieldValues.SetCustomerFieldValue;
using SubscriptionCommerce.Modules.Customers.Domain.Fields;
using SubscriptionCommerce.Modules.Customers.Domain.FieldValues;
using SubscriptionCommerce.Modules.Customers.Domain.Users;
using SubscriptionCommerce.Modules.Customers.IntegrationTests.Abstractions;

namespace SubscriptionCommerce.Modules.Customers.IntegrationTests.FieldValues;

public sealed class SetCustomerFieldValueTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_WriteHistoryOnlyWhenTheValueChanges()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        Result<SetCustomerFieldValueResponse> first = await this.SetTextAsync(tenantId, customerId, fieldId, "Gold", rowVersion: null);
        Result<SetCustomerFieldValueResponse> repeat = await this.SetTextAsync(tenantId, customerId, fieldId, "Gold", first.Value.RowVersion);

        first.Value.Changed.Should().BeTrue();
        repeat.Value.Changed.Should().BeFalse();
        repeat.Value.RowVersion.Should().Be(first.Value.RowVersion);
        (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_Return409ProblemDetails_WhenTheRowVersionIsStale()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();
        string url = ValueUrl(tenantId, customerId, fieldId);

        SetCustomerFieldValueResponse first = await PutTextAsync(url, "Gold", rowVersion: null);
        await PutTextAsync(url, "Platinum", first.RowVersion);

        using HttpResponseMessage stale = await HttpClient.PutAsJsonAsync(url, new { textValue = "Silver", rowVersion = first.RowVersion });

        stale.StatusCode.Should().Be(HttpStatusCode.Conflict);
        stale.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        ProblemDetails? problem = await stale.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(CustomerFieldValueErrors.ConcurrencyConflict.Code);
        (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Select(h => h.NewValue.Text).Should().Equal("Gold", "Platinum");
    }

    [Fact]
    public async Task Should_LetOnlyOneOfTwoConcurrentFirstValuesWin()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        Result<SetCustomerFieldValueResponse>[] results = await Task.WhenAll(
            this.SetTextAsync(tenantId, customerId, fieldId, "Gold", rowVersion: null),
            this.SetTextAsync(tenantId, customerId, fieldId, "Platinum", rowVersion: null));

        results.Should().ContainSingle(r => r.IsSuccess);
        results.Single(r => r.IsFailure).Error.Should().Be(CustomerFieldValueErrors.ConcurrencyConflict);
        (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_KeepHistoryInOrder_WithEachOldValueEqualToThePreviousNewValue()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        string? rowVersion = null;
        foreach (string text in new[] { "Gold", "Platinum", "Silver" })
        {
            rowVersion = (await this.SetTextAsync(tenantId, customerId, fieldId, text, rowVersion)).Value.RowVersion;
        }

        IReadOnlyList<FieldValueHistoryResponse> history = await this.GetHistoryAsync(tenantId, customerId, fieldId);

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
        int email = await this.AddOptionAsync(tenantId, fieldId, "Email", sortOrder: 0);
        int phone = await this.AddOptionAsync(tenantId, fieldId, "Phone", sortOrder: 1);

        Result<SetCustomerFieldValueResponse> result =
            await this.SetOptionsAsync(tenantId, customerId, fieldId, [phone, email], rowVersion: null);

        result.IsSuccess.Should().BeTrue();
        FieldValueHistoryResponse entry = (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Single();
        entry.NewValue.Options.Should().BeEquivalentTo(
            [new SnapshotOption(email, "Email"), new SnapshotOption(phone, "Phone")],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Should_Return404_ForAFieldOfAnotherTenant()
    {
        (int tenantId, int customerId, _) = await CreateCustomerWithTextFieldAsync();
        int otherTenantId = await this.CreateTenantAsync();
        int otherTenantsField = await this.CreateFieldAsync(otherTenantId, FieldType.Text);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            ValueUrl(tenantId, customerId, otherTenantsField),
            new { textValue = "Gold" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return404_ForACustomerOfAnotherTenant()
    {
        (_, int customerId, _) = await CreateCustomerWithTextFieldAsync();
        int otherTenantId = await this.CreateTenantAsync();
        int otherTenantsField = await this.CreateFieldAsync(otherTenantId, FieldType.Text);

        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            ValueUrl(otherTenantId, customerId, otherTenantsField),
            new { textValue = "Gold" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_Return404_ForHistoryRequestedThroughAnotherTenant()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();
        await this.SetTextAsync(tenantId, customerId, fieldId, "Gold", rowVersion: null);
        int otherTenantId = await this.CreateTenantAsync();

        using HttpResponseMessage response = await HttpClient.GetAsync(
            $"tenants/{otherTenantId}/customers/{customerId}/fields/{fieldId}/history");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_MoveTheRowVersionOn_WhenOnlyTheSelectedOptionsChange()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Dropdown, name: "Contact");
        int email = await this.AddOptionAsync(tenantId, fieldId, "Email", sortOrder: 0);
        int phone = await this.AddOptionAsync(tenantId, fieldId, "Phone", sortOrder: 1);

        Clock.FrozenAt = DateTimeOffset.UtcNow;
        try
        {
            string? first = (await this.SetOptionsAsync(tenantId, customerId, fieldId, [email], rowVersion: null)).Value.RowVersion;

            Result<SetCustomerFieldValueResponse> swapped = await this.SetOptionsAsync(tenantId, customerId, fieldId, [phone], first);
            Result<SetCustomerFieldValueResponse> stale = await this.SetOptionsAsync(tenantId, customerId, fieldId, [email], first);

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
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        using HttpResponseMessage response = await PutTextAsUserAsync(ValueUrl(tenantId, customerId, fieldId), "Gold", "maria");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Single().ChangedBy.Should().Be("maria");
    }

    [Fact]
    public async Task Should_Return400_WhenTheUserNameIsLongerThanTheAuditColumns()
    {
        (int tenantId, int customerId, int fieldId) = await CreateCustomerWithTextFieldAsync();

        using HttpResponseMessage response = await PutTextAsUserAsync(
            ValueUrl(tenantId, customerId, fieldId),
            "Gold",
            new string('u', UserName.MaxLength + 1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(UserErrors.NameTooLong.Code);
        (await this.GetHistoryAsync(tenantId, customerId, fieldId)).Should().BeEmpty();
    }

    private static string ValueUrl(int tenantId, int customerId, int fieldId) =>
        $"tenants/{tenantId}/customers/{customerId}/fields/{fieldId}/value";

    private async Task<(int TenantId, int CustomerId, int FieldId)> CreateCustomerWithTextFieldAsync()
    {
        int tenantId = await this.CreateTenantAsync();
        int customerId = await this.CreateCustomerAsync(tenantId);
        int fieldId = await this.CreateFieldAsync(tenantId, FieldType.Text);

        return (tenantId, customerId, fieldId);
    }

    private async Task<SetCustomerFieldValueResponse> PutTextAsync(string url, string text, string? rowVersion)
    {
        using HttpResponseMessage response = await HttpClient.PutAsJsonAsync(url, new { textValue = text, rowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<SetCustomerFieldValueResponse>())!;
    }

    private async Task<HttpResponseMessage> PutTextAsUserAsync(string url, string text, string userName)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(new { textValue = text })
        };
        request.Headers.Add("X-User-Name", userName);

        return await HttpClient.SendAsync(request);
    }
}
