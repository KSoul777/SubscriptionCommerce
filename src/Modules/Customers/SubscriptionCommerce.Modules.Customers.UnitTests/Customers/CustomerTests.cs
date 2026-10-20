using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using static SubscriptionCommerce.Modules.Customers.UnitTests.Abstractions.Fields;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_Should_BelongToItsTenantAndTrimTheName()
    {
        Result<Customer> result = Customer.Create(7, "  Fabrikam ", UtcNow);

        result.Value.TenantId.Should().Be(7);
        result.Value.Name.Should().Be("Fabrikam");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenTheNameIsMissing(string? name)
    {
        Result<Customer> result = Customer.Create(7, name, UtcNow);

        result.Error.Should().Be(CustomerErrors.NameInvalid);
    }

    [Fact]
    public void Create_Should_Fail_WhenTheNameIsLongerThanItsColumn()
    {
        Result<Customer> result = Customer.Create(7, new string('x', Customer.NameMaxLength + 1), UtcNow);

        result.Error.Should().Be(CustomerErrors.NameInvalid);
    }
}
