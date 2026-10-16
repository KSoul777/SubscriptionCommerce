using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using SubscriptionCommerce.Common.Infrastructure.Authentication;

namespace SubscriptionCommerce.Common.UnitTests.Authentication;

public class HeaderCurrentUserTests
{
    [Fact]
    public void UserName_Should_BeTheTrimmedHeaderValue()
    {
        HeaderCurrentUser user = UserWithHeader("  maria ");

        user.UserName.Should().Be("maria");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UserName_Should_BeAnonymous_WhenTheHeaderIsMissingOrBlank(string? header)
    {
        HeaderCurrentUser user = UserWithHeader(header);

        user.UserName.Should().Be("anonymous");
    }

    [Fact]
    public void UserName_Should_BeAnonymous_OutsideAnHttpRequest()
    {
        var user = new HeaderCurrentUser(new HttpContextAccessor { HttpContext = null });

        user.UserName.Should().Be("anonymous");
    }

    private static HeaderCurrentUser UserWithHeader(string? value)
    {
        var context = new DefaultHttpContext();

        if (value is not null)
        {
            context.Request.Headers[HeaderCurrentUser.HeaderName] = value;
        }

        return new HeaderCurrentUser(new HttpContextAccessor { HttpContext = context });
    }
}
