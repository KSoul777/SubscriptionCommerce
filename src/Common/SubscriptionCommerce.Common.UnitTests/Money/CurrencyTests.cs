using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Common.UnitTests.Money;

public class CurrencyTests
{
    [Theory]
    [InlineData("EUR")]
    [InlineData(" usd ")]
    [InlineData("Gbp")]
    public void Create_Should_AcceptASupportedCurrency_IgnoringCaseAndSpaces(string isoCode)
    {
        Result<Currency> result = Currency.Create(isoCode);

        result.Value.IsoCode.Should().Be(isoCode.Trim().ToUpperInvariant());
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("BHD")]
    [InlineData("XYZ")]
    public void Create_Should_Fail_WhenTheCurrencyIsNotSupported(string isoCode)
    {
        Result<Currency> result = Currency.Create(isoCode);

        result.Error.Should().Be(MoneyErrors.CurrencyNotSupported);
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    public void Create_Should_Fail_WhenTheCodeIsNotThreeLetters(string isoCode)
    {
        Result<Currency> result = Currency.Create(isoCode);

        result.Error.Should().Be(MoneyErrors.CurrencyInvalid);
    }
}
