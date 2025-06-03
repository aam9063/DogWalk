using ArchitectureTests.Infrastructure;
using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests.Application;

public class ApplicationTest : BaseTest
{
    [Fact]
    public void CommandHandler_Should_NotBePublic()
    {
        var resultados = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<>))
            .Or()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult();

        resultados.IsSuccessful.Should().BeTrue();   
    }
}
