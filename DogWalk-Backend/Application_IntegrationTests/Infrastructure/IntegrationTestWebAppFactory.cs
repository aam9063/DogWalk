using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Xunit;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application_IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithName("DogWalk")
        .WithPassword("2ZE868Fru")
        .WithPortBinding(1433)
        .Build();

    // Inicializa la base de datos
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    // Destruye la base de datos
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => {
            services.RemoveAll(typeof(DbContextOptions<DogWalkDbContext>));
            
            // Modificar la cadena de conexión para incluir el nombre de la base de datos
            var connectionString = $"{_dbContainer.GetConnectionString()};Database=DogWalk_Tests";
            
            services.AddDbContext<DogWalkDbContext>(options => {
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
