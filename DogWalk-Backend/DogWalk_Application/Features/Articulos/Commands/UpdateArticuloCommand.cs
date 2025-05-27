using DogWalk_Application.Contracts.DTOs.Articulos;
using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

/// <summary>
/// Comando para actualizar un artículo.
/// </summary>
public class UpdateArticuloCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public UpdateArticuloDto ArticuloDto { get; set; }
}
