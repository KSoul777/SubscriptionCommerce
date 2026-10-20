using NetArchTest.Rules;
using SubscriptionCommerce.ArchitectureTests.Abstractions;

namespace SubscriptionCommerce.ArchitectureTests.Boundaries;

public class ModuleTests : BaseTest
{
    [Fact]
    public void PricingModule_Should_NotHaveDependencyOn_CustomersModule()
    {
        Types.InAssemblies(Pricing.All)
            .Should()
            .NotHaveDependencyOn(CustomersNamespace)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void CustomersModule_Should_NotHaveDependencyOn_PricingModule()
    {
        Types.InAssemblies(Customers.All)
            .Should()
            .NotHaveDependencyOn(PricingNamespace)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Common_Should_NotHaveDependencyOn_AnyModule()
    {
        Types.InAssemblies(CommonAssemblies)
            .Should()
            .NotHaveDependencyOn(ModulesNamespace)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
