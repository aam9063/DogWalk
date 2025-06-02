using System;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using FluentAssertions;
using Xunit;

namespace DogWalk_Domain_UnitTests.Users;

public class UserTests
{
    [Fact]
    public void CreateShould_SetPropertyValues()
    {
        // Arrange (Data de prueba)


        // Act (Ejecutar el metodo a probar)
        var user = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);

        // Assert (Verificar el resultado)
        user.Nombre.Should().Be(UserMock.Nombre);
        user.Apellido.Should().Be(UserMock.Apellido);
        user.Email.Should().Be(UserMock.Email);
        user.Password.Should().Be(UserMock.Password);
    }

    [Fact]
    public void Constructor_DebeEstablecerPropiedadesCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var usuario = new Usuario(id, null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);

        // Assert
        Assert.Equal(id, usuario.Id);
        Assert.Equal(UserMock.Nombre, usuario.Nombre);
        Assert.Equal(UserMock.Apellido, usuario.Apellido);
        Assert.Equal(UserMock.Email, usuario.Email);
        Assert.Equal(UserMock.Password, usuario.Password);
        Assert.Equal(RolUsuario.Usuario, usuario.Rol); // Valor por defecto
    }

    [Fact]
    public void ActualizarDatos_DebeModificarPropiedadesCorrectamente()
    {
        // Arrange
        var usuario = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);
        var nuevoNombre = "Carlos";
        var nuevoApellido = "Pérez";
        var nuevaDireccion = Direccion.Create("Calle Test 123");
        var nuevoTelefono = Telefono.Create("123456789");

        // Act
        usuario.ActualizarDatos(nuevoNombre, nuevoApellido, nuevaDireccion, nuevoTelefono);

        // Assert
        Assert.Equal(nuevoNombre, usuario.Nombre);
        Assert.Equal(nuevoApellido, usuario.Apellido);
        Assert.Equal(nuevaDireccion, usuario.Direccion);
        Assert.Equal(nuevoTelefono, usuario.Telefono);
    }

    [Fact]
    public void CambiarPassword_ConPasswordCorrecta_DebeCambiarPassword()
    {
        // Arrange
        var usuario = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);
        var nuevaPassword = "NuevaPassword123";

        // Act
        usuario.CambiarPassword("Test1234", nuevaPassword);

        // Assert
        Assert.True(usuario.Password.Verify(nuevaPassword));
    }

    [Fact]
    public void CambiarPassword_ConPasswordIncorrecta_DebeLanzarExcepcion()
    {
        // Arrange
        var usuario = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            usuario.CambiarPassword("PasswordIncorrecta", "NuevaPassword123"));
    }

    [Fact]
    public void AgregarPerro_DebeAgregarPerroALaLista()
    {
        // Arrange
        var usuario = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);
        var perro = new Perro(Guid.NewGuid(), usuario.Id, "Firulais", "Labrador", 3);

        // Act
        usuario.AgregarPerro(perro);

        // Assert
        Assert.Single(usuario.Perros);
        Assert.Contains(perro, usuario.Perros);
    }

    [Fact]
    public void AgregarItemCarrito_ItemNuevo_DebeAgregarAlCarrito()
    {
        // Arrange
        var usuario = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);
        var item = new ItemCarrito(
            Guid.NewGuid(),
            usuario.Id,
            Guid.NewGuid(), // ArticuloId
            1, // cantidad
            Dinero.Create(10.99m) // precioUnitario
        );

        // Act
        usuario.AgregarItemCarrito(item);

        // Assert
        Assert.Single(usuario.Carrito);
        Assert.Contains(item, usuario.Carrito);
    }

    [Fact]
    public void VaciarCarrito_DebeEliminarTodosLosItems()
    {
        // Arrange
        var usuario = new Usuario(Guid.NewGuid(), null, UserMock.Nombre, UserMock.Apellido, null, UserMock.Email, UserMock.Password, null);
        usuario.AgregarItemCarrito(new ItemCarrito(
            Guid.NewGuid(),
            usuario.Id,
            Guid.NewGuid(),
            1,
            Dinero.Create(10.99m)
        ));
        usuario.AgregarItemCarrito(new ItemCarrito(
            Guid.NewGuid(),
            usuario.Id,
            Guid.NewGuid(),
            1,
            Dinero.Create(15.99m)
        ));

        // Act
        usuario.VaciarCarrito();

        // Assert
        Assert.Empty(usuario.Carrito);
    }
}
