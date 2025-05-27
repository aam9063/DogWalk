using DogWalk_Application.Contracts.DTOs.Articulos;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Queries
{
    /// <summary>
    /// Manejador para obtener un artículo por su ID.
    /// </summary>
    public class GetArticuloByIdQueryHandler : IRequestHandler<GetArticuloByIdQuery, ArticuloDetailsDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de consultas de artículo por ID.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public GetArticuloByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja la consulta para obtener un artículo por su ID.
        /// </summary>
        /// <param name="request">Consulta para obtener un artículo por su ID.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<ArticuloDetailsDto> Handle(GetArticuloByIdQuery request, CancellationToken cancellationToken)
        {
            var articulo = await _unitOfWork.Articulos.GetByIdAsync(request.Id);
            
            if (articulo == null)
                return null;
                
            return new ArticuloDetailsDto
            {
                Id = articulo.Id,
                Nombre = articulo.Nombre,
                Descripcion = articulo.Descripcion,
                Precio = articulo.Precio.Cantidad,
                Stock = articulo.Stock,
                Categoria = articulo.Categoria.ToString(),
                FechaCreacion = articulo.CreadoEn,
                FechaModificacion = articulo.ModificadoEn,
                Imagenes = articulo.Imagenes?.Select(i => i.UrlImagen).ToList() ?? new List<string>()
            };
        }
    }
}
