using System.Globalization;

namespace SubscriptionCommerce.Modules.Customers.Infrastructure.Database;

internal static class CanonicalText
{
    internal const string BinaryCollation = "Latin1_General_100_BIN2";

    private static readonly string WhiteSpaceCodePoints = string.Join(
        ", ",
        Enumerable.Range(0, char.MaxValue + 1)
            .Where(c => char.IsWhiteSpace((char)c))
            .Select(c => c.ToString(CultureInfo.InvariantCulture)));

    internal static string KeyOf(string column) => $"UPPER([{column}] COLLATE {BinaryCollation})";

    internal static string IsCanonical(string column) =>
        $"DATALENGTH([{column}]) > 0 AND " +
        $"UNICODE(LEFT([{column}], 1)) NOT IN ({WhiteSpaceCodePoints}) AND " +
        $"UNICODE(RIGHT([{column}], 1)) NOT IN ({WhiteSpaceCodePoints})";
}
