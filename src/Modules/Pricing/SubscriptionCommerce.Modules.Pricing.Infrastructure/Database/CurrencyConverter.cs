using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Modules.Pricing.Infrastructure.Database;

internal sealed class CurrencyConverter() : ValueConverter<Currency, string>(
    currency => currency.IsoCode,
    isoCode => FromStored(isoCode))
{
    private static Currency FromStored(string isoCode)
    {
        Result<Currency> currency = Currency.Create(isoCode);

        return currency.IsSuccess
            ? currency.Value
            : throw new InvalidOperationException(
                $"The stored currency '{isoCode}' is not a supported currency: {currency.Error.Description}");
    }
}
