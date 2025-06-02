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
using Microsoft.EntityFrameworkCore;

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
            try
            {
                // Crear expresión de filtro base
                Expression<Func<DogWalk_Domain.Entities.Articulo, bool>> predicate = a => true;
                
                // Aplicar filtros de manera más eficiente
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    predicate = a => EF.Functions.Like(a.Nombre.ToLower(), $"%{searchTerm}%") || 
                                    EF.Functions.Like(a.Descripcion.ToLower(), $"%{searchTerm}%");
                }
                
                if (request.Categoria.HasValue)
                {
                    var categoria = request.Categoria.Value;
                    predicate = a => a.Categoria == categoria;
                }
                
                // Determinar ordenamiento
                Expression<Func<DogWalk_Domain.Entities.Articulo, object>> orderBy = request.SortBy?.ToLower() switch
                {
                    "precio" => a => a.Precio.Cantidad,
                    "stock" => a => a.Stock,
                    _ => a => a.Nombre
                };
                
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
                
                return new ResultadoPaginadoDto<ArticuloDto>
                {
                    Items = articulosDto,
                    TotalItems = totalItems,
                    TotalPaginas = (int)Math.Ceiling(totalItems / (double)request.PageSize),
                    PaginaActual = request.PageNumber,
                    ElementosPorPagina = request.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al obtener artículos: {ex.Message}", ex);
            }
        }
    }
}
