using AwesomeAssertions;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Presentation.Results;

namespace SubscriptionCommerce.Common.UnitTests.Results;

public class ApiResultsTests
{
    [Fact]
    public void Problem_Should_Throw_WhenTheResultIsASuccess()
    {
        Action problem = () => ApiResults.Problem(Result.Success());

        problem.Should().Throw<InvalidOperationException>()
            .WithMessage("A problem response can't be created from a successful result.");
    }
}
