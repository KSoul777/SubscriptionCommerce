using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Users;

public static class UserErrors
{
    public static readonly Error NameRequired = Error.Problem(
        "Users.NameRequired",
        "The user name is required");

    public static readonly Error NameTooLong = Error.Problem(
        "Users.NameTooLong",
        $"The user name must be at most {UserName.MaxLength} characters");
}
