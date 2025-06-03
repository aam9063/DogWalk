using Application_IntegrationTests.Infrastructure;
using DogWalk_Application.Features.Paseadores.Queries;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Common.ValueObjects;
using Xunit;

namespace Application_IntegrationTests.Paseadores;

public class BuscarPaseadores : BaseIntegrationTest
{
    public BuscarPaseadores(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task BuscarPaseadores_ShouldReturnPaseadores()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dni = Dni.Create("12345678A");
        var nombre = "Test";
        var apellido = "Paseador";
        var direccion = Direccion.Create("Calle Test", "Ciudad Test", "12345");
        var email = Email.Create("test@test.com");
        var password = Password.Create("Test1234!");
        var ubicacion = Coordenadas.Create(40.416775, -3.703790);

        var paseador = new Paseador(
            id,
            dni,
            nombre,
            apellido,
            direccion,
            email,
            password,
            ubicacion
        );

        await dbContext.Paseadores.AddAsync(paseador);
        await dbContext.SaveChangesAsync();

        var query = new BuscarPaseadoresQuery();

        // Act
        var result = await Sender.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
    }
}
