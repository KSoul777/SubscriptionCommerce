using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Customers.Domain.Users;

public sealed record UserName
{
    public const int MaxLength = 200;

    private UserName(string value) => Value = value;

    public string Value { get; }

    public static Result<UserName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<UserName>(UserErrors.NameRequired);
        }

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<UserName>(UserErrors.NameTooLong);
        }

        return new UserName(trimmed);
    }

    public override string ToString() => Value;
}
