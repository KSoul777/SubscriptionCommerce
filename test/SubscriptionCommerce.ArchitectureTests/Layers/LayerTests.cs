using System.Reflection;
using NetArchTest.Rules;
using SubscriptionCommerce.ArchitectureTests.Abstractions;

namespace SubscriptionCommerce.ArchitectureTests.Layers;

// Inside a module, dependencies point inward: Presentation -> Application -> Domain,
// and Infrastructure is the only layer that knows about EF Core and SQL Server.
public class LayerTests : BaseTest
{
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void DomainLayer_Should_NotHaveDependencyOn_OuterLayers(string module)
    {
        ModuleAssemblies assemblies = Module(module);

        Types.InAssembly(assemblies.Domain)
            .Should()
            .NotHaveDependencyOnAny(NamesOf(assemblies.Application, assemblies.Presentation, assemblies.Infrastructure))
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void ApplicationLayer_Should_NotHaveDependencyOn_PresentationOrInfrastructure(string module)
    {
        ModuleAssemblies assemblies = Module(module);

        Types.InAssembly(assemblies.Application)
            .Should()
            .NotHaveDependencyOnAny(NamesOf(assemblies.Presentation, assemblies.Infrastructure))
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void PresentationLayer_Should_NotHaveDependencyOn_Infrastructure(string module)
    {
        ModuleAssemblies assemblies = Module(module);

        Types.InAssembly(assemblies.Presentation)
            .Should()
            .NotHaveDependencyOn(assemblies.Infrastructure.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void DomainAndApplicationLayers_Should_NotHaveDependencyOn_EntityFrameworkCore(string module)
    {
        ModuleAssemblies assemblies = Module(module);

        Types.InAssemblies([assemblies.Domain, assemblies.Application])
            .Should()
            .NotHaveDependencyOn(EntityFrameworkCore)
            .GetResult()
            .ShouldBeSuccessful();
    }

    private static string[] NamesOf(params Assembly[] assemblies) =>
        assemblies.Select(a => a.GetName().Name!).ToArray();
}
