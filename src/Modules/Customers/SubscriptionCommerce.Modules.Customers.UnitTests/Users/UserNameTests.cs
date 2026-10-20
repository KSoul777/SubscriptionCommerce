using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Modules.Customers.Domain.Users;

namespace SubscriptionCommerce.Modules.Customers.UnitTests.Users;

public class UserNameTests
{
    [Fact]
    public void Create_Should_TrimTheName()
    {
        Result<UserName> result = UserName.Create("  maria ");

        result.Value.Value.Should().Be("maria");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_Fail_WhenTheNameIsMissing(string? name)
    {
        Result<UserName> result = UserName.Create(name);

        result.Error.Should().Be(UserErrors.NameRequired);
    }

    [Fact]
    public void Create_Should_Fail_WhenTheNameIsLongerThanTheAuditColumns()
    {
        Result<UserName> result = UserName.Create(new string('u', UserName.MaxLength + 1));

        result.Error.Should().Be(UserErrors.NameTooLong);
    }

    [Fact]
    public void Create_Should_AcceptANameOfExactlyTheMaximumLength()
    {
        Result<UserName> result = UserName.Create(new string('u', UserName.MaxLength));

        result.IsSuccess.Should().BeTrue();
    }
}
