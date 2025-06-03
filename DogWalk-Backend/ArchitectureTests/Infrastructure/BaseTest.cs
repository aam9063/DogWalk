
using System.Reflection;
using DogWalk_Application.Common.Behaviors;
using DogWalk_Domain.Entities;
using DogWalk_Infrastructure.Persistence.Context;

namespace ArchitectureTests.Infrastructure;

public class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(ValidationBehavior<,>).Assembly;
    protected static readonly Assembly DomainAssembly = typeof(EntityBase).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(DogWalkDbContext).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(Program).Assembly;
}


   