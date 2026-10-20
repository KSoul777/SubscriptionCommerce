using Microsoft.AspNetCore.Http;
using SubscriptionCommerce.Common.Application.Authentication;

namespace SubscriptionCommerce.Common.Infrastructure.Authentication;

internal sealed class HeaderCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    internal const string HeaderName = "X-User-Name";
    private const string Anonymous = "anonymous";

    public string UserName
    {
        get
        {
            string? userName = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();

            return string.IsNullOrWhiteSpace(userName) ? Anonymous : userName.Trim();
        }
    }
}
