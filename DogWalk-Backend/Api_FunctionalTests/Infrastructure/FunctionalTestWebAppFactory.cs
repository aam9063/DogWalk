using System.Net.Http.Json;
using Api_FunctionalTests.Users;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Xunit;

namespace Api_FunctionalTests.Infrastructure;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithName("DogWalk")
        .WithPassword(Environment.GetEnvironmentVariable("TEST_SQL_PASSWORD"))
        .WithPortBinding(1433)
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();

        await CreateUserTestAsync();
    }

    private async Task CreateUserTestAsync()
    {
        var httpClient = CreateClient();

        await httpClient
            .PostAsJsonAsync("api/Usuario/register", UserData.RegisterUserRquestTest);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<DogWalkDbContext>));

            // Modificar la cadena de conexión para incluir el nombre de la base de datos
            var connectionString = $"{_dbContainer.GetConnectionString()};Database=DogWalk_Tests";

            services.AddDbContext<DogWalkDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                // Ignorar la advertencia de cambios pendientes en el modelo
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            // Crear un scope para aplicar las migraciones
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<DogWalkDbContext>();

                // Crear la base de datos y aplicar migraciones
                db.Database.Migrate();
            }
        });
        base.ConfigureWebHost(builder);
    }
}
