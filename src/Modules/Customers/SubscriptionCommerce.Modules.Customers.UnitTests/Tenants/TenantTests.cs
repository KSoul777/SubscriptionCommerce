using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Tenants;
using static SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions.Fields;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Tenants;

public class TenantTests
{
    [Fact]
    public void Create_Should_TrimTheName()
    {
        Result<Tenant> result = Tenant.Create("  Contoso ", UtcNow);

        result.Value.Name.Should().Be("Contoso");
        result.Value.CreatedAtUtc.Should().Be(UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenTheNameIsMissing(string? name)
    {
        Result<Tenant> result = Tenant.Create(name, UtcNow);

        result.Error.Should().Be(TenantErrors.NameInvalid);
    }

    [Fact]
    public void Create_Should_Fail_WhenTheNameIsLongerThanItsColumn()
    {
        Result<Tenant> result = Tenant.Create(new string('x', Tenant.NameMaxLength + 1), UtcNow);

        result.Error.Should().Be(TenantErrors.NameInvalid);
    }
}
