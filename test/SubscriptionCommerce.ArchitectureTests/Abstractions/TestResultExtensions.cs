using AwesomeAssertions;
using NetArchTest.Rules;

namespace SubscriptionCommerce.ArchitectureTests.Abstractions;

internal static class TestResultExtensions
{
    internal static void ShouldBeSuccessful(this TestResult testResult) =>
        testResult.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}",
            string.Join(", ", testResult.FailingTypeNames ?? []));
}
