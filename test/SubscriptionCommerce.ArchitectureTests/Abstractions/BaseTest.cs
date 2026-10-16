using System.Reflection;
using SubscriptionCommerce.Common.Application;
using SubscriptionCommerce.Common.Domain;
using SubscriptionCommerce.Common.Infrastructure;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Modules.Customers.Domain.Customers;
using SubscriptionCommerce.Modules.Customers.Infrastructure;
using SubscriptionCommerce.Modules.Pricing.Domain.Discounts;
using SubscriptionCommerce.Modules.Pricing.Infrastructure;

namespace SubscriptionCommerce.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected const string ModulesNamespace = "SubscriptionCommerce.Modules";
    protected const string PricingNamespace = "SubscriptionCommerce.Modules.Pricing";
    protected const string CustomersNamespace = "SubscriptionCommerce.Modules.Customers";

    protected static readonly Assembly[] CommonAssemblies =
    [
        typeof(Result).Assembly,
        typeof(ApplicationConfiguration).Assembly,
        typeof(InfrastructureConfiguration).Assembly,
        typeof(IEndpoint).Assembly
    ];

    protected static readonly ModuleAssemblies Pricing = new(
        typeof(Discount).Assembly,
        Modules.Pricing.Application.AssemblyReference.Assembly,
        Modules.Pricing.Presentation.AssemblyReference.Assembly,
        typeof(PricingModule).Assembly);

    protected static readonly ModuleAssemblies Customers = new(
        typeof(Customer).Assembly,
        Modules.Customers.Application.AssemblyReference.Assembly,
        Modules.Customers.Presentation.AssemblyReference.Assembly,
        typeof(CustomersModule).Assembly);

    // Theory data is the module name, so xUnit can show one test case per module.
    public static TheoryData<string> ModuleNames => ["Pricing", "Customers"];

    protected static ModuleAssemblies Module(string name) => name switch
    {
        "Pricing" => Pricing,
        "Customers" => Customers,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown module")
    };
}

public sealed record ModuleAssemblies(
    Assembly Domain,
    Assembly Application,
    Assembly Presentation,
    Assembly Infrastructure)
{
    public Assembly[] All => [Domain, Application, Presentation, Infrastructure];
}
