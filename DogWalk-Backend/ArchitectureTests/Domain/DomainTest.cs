using System.Reflection;
using ArchitectureTests.Infrastructure;
using DogWalk_Domain.Entities;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests.Domain;

public class DomainTest : BaseTest
{
    [Fact]
    public void Entities_ShouldHave_PrivateConstructorsNoParameters()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(EntityBase))
            .GetTypes();

        var errorEntities = new List<Type>();

        foreach(var entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if(!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                errorEntities.Add(entityType);
            } 
        }

        errorEntities.Should().BeEmpty();
    }
}