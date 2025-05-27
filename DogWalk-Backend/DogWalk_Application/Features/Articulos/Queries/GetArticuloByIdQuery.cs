using DogWalk_Application.Contracts.DTOs.Articulos;
using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Queries;

/// <summary>
/// Consulta para obtener un artículo por su ID.
/// </summary>
public class GetArticuloByIdQuery : IRequest<ArticuloDetailsDto>
{
    public Guid Id { get; set; }
}
