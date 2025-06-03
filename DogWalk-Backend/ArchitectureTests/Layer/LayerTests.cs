
using ArchitectureTests.Infrastructure;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests.Layer;

public class LayerTests : BaseTest
{
    [Fact]
    public void Domain_ShouldHaveNotDependency_ApplicationLayer()
    {
        var resultados = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult();
        
        resultados.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldHaveNotDependency_InfrastructureLayer()
    {
        var resultados = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();
        
        resultados.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldHaveNotDependency_InfrastructureLayer()
    {
        var resultados = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();
        
        resultados.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldHaveNotDependency_PresentationLayer()
    {
        var resultados = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();
        
        resultados.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void InfrastructureLayer_ShouldHaveNotDependency_PresentationLayer()
    {
        var resultados = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(ApiAssembly.GetName().Name)
            .GetResult();
        
        resultados.IsSuccessful.Should().BeTrue();
    }
}
