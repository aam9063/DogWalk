using DogWalk_Application.Contracts.DTOs.Articulos;
using DogWalk_Application.Contracts.DTOs.Busqueda;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Queries
{
    /// <summary>
    /// Manejador para obtener todos los artículos.
    /// </summary>
    public class GetAllArticulosQueryHandler : IRequestHandler<GetAllArticulosQuery, ResultadoPaginadoDto<ArticuloDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de consultas de artículos.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public GetAllArticulosQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja la consulta para obtener todos los artículos.
        /// </summary>
        /// <param name="request">Consulta para obtener todos los artículos.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<ResultadoPaginadoDto<ArticuloDto>> Handle(GetAllArticulosQuery request, CancellationToken cancellationToken)
        {
            // Crear expresión de filtro base
            Expression<Func<DogWalk_Domain.Entities.Articulo, bool>> predicate = a => true;
            
            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                predicate = a => a.Nombre.Contains(request.SearchTerm) || 
                                a.Descripcion.Contains(request.SearchTerm);
            }
            
            // Aplicar filtro de categoría si existe
            if (request.Categoria.HasValue)
            {
                var categoria = request.Categoria.Value;
                Expression<Func<DogWalk_Domain.Entities.Articulo, bool>> categoriaPredicate = 
                    a => a.Categoria == categoria;
                
                // Combinar predicados
                var parameter = Expression.Parameter(typeof(DogWalk_Domain.Entities.Articulo), "a");
                var body = Expression.AndAlso(
                    Expression.Invoke(predicate, parameter),
                    Expression.Invoke(categoriaPredicate, parameter)
                );
                predicate = Expression.Lambda<Func<DogWalk_Domain.Entities.Articulo, bool>>(body, parameter);
            }
            
            // Determinar ordenamiento
            Expression<Func<DogWalk_Domain.Entities.Articulo, object>> orderBy = a => a.Nombre;
            
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                orderBy = request.SortBy.ToLower() switch
                {
                    "precio" => a => a.Precio.Cantidad,
                    "stock" => a => a.Stock,
                    _ => a => a.Nombre
                };
            }
            
            // Ejecutar consulta paginada
            var (articulos, totalItems) = await _unitOfWork.Articulos.GetPagedAsync(
                predicate, 
                orderBy, 
                request.Ascending, 
                request.PageNumber, 
                request.PageSize);
            
            // Mapear a DTOs
            var articulosDto = articulos.Select(a => new ArticuloDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Precio = a.Precio.Cantidad,
                Stock = a.Stock,
                Categoria = a.Categoria.ToString(),
                ImagenPrincipal = a.Imagenes?.FirstOrDefault()?.UrlImagen ?? string.Empty,
                FechaCreacion = a.CreadoEn
            }).ToList();
            
            // Calcular valores de paginación
            int totalPaginas = (int)Math.Ceiling(totalItems / (double)request.PageSize);
            
            // Devolver resultado paginado
            return new ResultadoPaginadoDto<ArticuloDto>
            {
                Items = articulosDto,
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                PaginaActual = request.PageNumber,
                ElementosPorPagina = request.PageSize
            };
        }
    }
}
