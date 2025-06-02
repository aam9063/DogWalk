using DogWalk_Application.Contracts.DTOs.Articulos;
using DogWalk_Application.Features.Articulos.Commands;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DogWalk_Application_UnitTests.Artículos
{
    public class CreateArticuloTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IArticuloRepository _articuloRepository;
        private readonly CreateArticuloCommandHandler _handler;

        public CreateArticuloTests()
        {
            _articuloRepository = Substitute.For<IArticuloRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _unitOfWork.Articulos.Returns(_articuloRepository);
            _handler = new CreateArticuloCommandHandler(_unitOfWork);
        }

        [Fact]
        public async Task Handle_ArticuloSinImagenes_DebeCrearArticuloCorrectamente()
        {
            // Arrange
            var command = new CreateArticuloCommand
            {
                ArticuloDto = new CreateArticuloDto
                {
                    Nombre = "Collar para perro",
                    Descripcion = "Collar ajustable",
                    Precio = 19.99m,
                    Stock = 10,
                    Categoria = (int)CategoriaArticulo.Accesorio,
                    Imagenes = new List<string>()
                }
            };

            Articulo? capturedArticulo = null;
            _articuloRepository
                .When(x => x.AddAsync(Arg.Any<Articulo>()))
                .Do(x => capturedArticulo = x.Arg<Articulo>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            await _articuloRepository.Received(1).AddAsync(Arg.Any<Articulo>());
            await _unitOfWork.Received(1).SaveChangesAsync();
            
            Assert.NotNull(capturedArticulo);
            Assert.Equal(command.ArticuloDto.Nombre, capturedArticulo.Nombre);
            Assert.Equal(command.ArticuloDto.Descripcion, capturedArticulo.Descripcion);
            Assert.Equal(command.ArticuloDto.Precio, capturedArticulo.Precio.Cantidad);
            Assert.Equal(command.ArticuloDto.Stock, capturedArticulo.Stock);
            Assert.Equal((CategoriaArticulo)command.ArticuloDto.Categoria, capturedArticulo.Categoria);
            Assert.Empty(capturedArticulo.Imagenes);
        }

        [Fact]
        public async Task Handle_ArticuloConImagenes_DebeCrearArticuloConImagenesCorrectamente()
        {
            // Arrange
            var command = new CreateArticuloCommand
            {
                ArticuloDto = new CreateArticuloDto
                {
                    Nombre = "Collar para perro",
                    Descripcion = "Collar ajustable",
                    Precio = 19.99m,
                    Stock = 10,
                    Categoria = (int)CategoriaArticulo.Accesorio,
                    Imagenes = new List<string> 
                    { 
                        "imagen1.jpg",
                        "imagen2.jpg" 
                    }
                }
            };

            Articulo? capturedArticulo = null;
            _articuloRepository
                .When(x => x.AddAsync(Arg.Any<Articulo>()))
                .Do(x => capturedArticulo = x.Arg<Articulo>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            await _articuloRepository.Received(1).AddAsync(Arg.Any<Articulo>());
            await _unitOfWork.Received(1).SaveChangesAsync();
            
            Assert.NotNull(capturedArticulo);
            Assert.Equal(2, capturedArticulo.Imagenes.Count);
            
            var imagenes = capturedArticulo.Imagenes.ToList();
            Assert.True(imagenes[0].EsPrincipal); // Primera imagen debe ser principal
            Assert.False(imagenes[1].EsPrincipal);
            Assert.Equal("imagen1.jpg", imagenes[0].UrlImagen);
            Assert.Equal("imagen2.jpg", imagenes[1].UrlImagen);
        }

        [Fact]
        public async Task Handle_ErrorAlGuardar_DebePropagarExcepcion()
        {
            // Arrange
            var command = new CreateArticuloCommand
            {
                ArticuloDto = new CreateArticuloDto
                {
                    Nombre = "Collar para perro",
                    Descripcion = "Collar ajustable",
                    Precio = 19.99m,
                    Stock = 10,
                    Categoria = (int)CategoriaArticulo.Accesorio
                }
            };

            _unitOfWork
                .When(x => x.SaveChangesAsync())
                .Do(x => { throw new Exception("Error de base de datos"); });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _handler.Handle(command, CancellationToken.None));
        }
    }
}