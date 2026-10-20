using FluentValidation;
using NetArchTest.Rules;
using SubscriptionCommerce.ArchitectureTests.Abstractions;
using SubscriptionCommerce.Common.Application.Messaging;

namespace SubscriptionCommerce.ArchitectureTests.Application;

public class ApplicationTests : BaseTest
{
    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Commands_Should_BeSealed_AndEndWithCommand(string module)
    {
        Types.InAssembly(Module(module).Application)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .BeSealed()
            .And()
            .HaveNameEndingWith("Command")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Queries_Should_BeSealed_AndEndWithQuery(string module)
    {
        Types.InAssembly(Module(module).Application)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeSealed()
            .And()
            .HaveNameEndingWith("Query")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void CommandHandlers_Should_BeInternalSealed_AndEndWithCommandHandler(string module)
    {
        Types.InAssembly(Module(module).Application)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .NotBePublic()
            .And()
            .BeSealed()
            .And()
            .HaveNameEndingWith("CommandHandler")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void QueryHandlers_Should_BeInternalSealed_AndEndWithQueryHandler(string module)
    {
        Types.InAssembly(Module(module).Application)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .And()
            .BeSealed()
            .And()
            .HaveNameEndingWith("QueryHandler")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Theory]
    [MemberData(nameof(ModuleNames))]
    public void Validators_Should_BeInternalSealed_AndEndWithValidator(string module)
    {
        Types.InAssembly(Module(module).Application)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .NotBePublic()
            .And()
            .BeSealed()
            .And()
            .HaveNameEndingWith("Validator")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
