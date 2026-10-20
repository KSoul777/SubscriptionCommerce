using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;

namespace SubscriptionCommerce.Common.UnitTests.Money;

public class MoneyTests
{
    private static readonly Currency Euro = Currency.Create("EUR").Value;

    [Fact]
    public void Create_Should_HoldTheAmountAndCurrency()
    {
        Result<Domain.Money> result = Domain.Money.Create(10m, Euro);

        result.Value.Amount.Should().Be(10m);
        result.Value.Currency.Should().Be(Euro);
    }

    [Fact]
    public void Create_Should_Fail_WhenTheCurrencyIsMissing()
    {
        Result<Domain.Money> result = Domain.Money.Create(10m, (Currency?)null);

        result.Error.Should().Be(MoneyErrors.CurrencyRequired);
    }
}
