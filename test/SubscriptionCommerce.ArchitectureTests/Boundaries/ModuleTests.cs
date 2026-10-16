using NetArchTest.Rules;
using SubscriptionCommerce.ArchitectureTests.Abstractions;

namespace SubscriptionCommerce.ArchitectureTests.Boundaries;

// Modules never reference each other. If one ever needs the other's data, it goes through a public
// contract project or an integration event (see the README), and these tests change on purpose.
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
