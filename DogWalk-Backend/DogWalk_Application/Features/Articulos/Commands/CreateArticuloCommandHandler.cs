using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands
{
    public class CreateArticuloCommandHandler : IRequestHandler<CreateArticuloCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateArticuloCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateArticuloCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ArticuloDto;
            
            // Crear el artículo
            var articulo = new Articulo(
                Guid.NewGuid(),
                dto.Nombre,
                dto.Descripcion,
                Dinero.Create(dto.Precio),
                dto.Stock,
                (CategoriaArticulo)dto.Categoria
            );
            
            // Agregar imágenes si existen
            if (dto.Imagenes != null && dto.Imagenes.Count > 0)
            {
                foreach (var urlImagen in dto.Imagenes)
                {
                    var imagen = new ImagenArticulo(
                        Guid.NewGuid(),
                        articulo.Id,
                        urlImagen,
                        dto.Imagenes.IndexOf(urlImagen) == 0 // Primera imagen como principal
                    );
                    
                    articulo.AgregarImagen(imagen);
                }
            }
            
            // Guardar en la base de datos
            await _unitOfWork.Articulos.AddAsync(articulo);
            await _unitOfWork.SaveChangesAsync();
            
            return articulo.Id;
        }
    }
} 