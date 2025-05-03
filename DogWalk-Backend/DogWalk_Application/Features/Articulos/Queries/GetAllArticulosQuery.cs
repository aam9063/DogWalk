using System;
using DogWalk_Application.Contracts.DTOs.Articulos;
using DogWalk_Application.Contracts.DTOs.Busqueda;
using DogWalk_Domain.Common.Enums;
using MediatR;

namespace DogWalk_Application.Features.Articulos.Queries;

public class GetAllArticulosQuery : IRequest<ResultadoPaginadoDto<ArticuloDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchTerm { get; set; }
    public string SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public CategoriaArticulo? Categoria { get; set; }
}
