using System;
using DogWalk_Infrastructure.Persistence.Context;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application_IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IServiceScope _scope;
    protected readonly DogWalkDbContext dbContext;
    protected readonly ISender Sender;


    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        dbContext = _scope.ServiceProvider.GetRequiredService<DogWalkDbContext>();
    }

}
