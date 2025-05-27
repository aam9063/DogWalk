using DogWalk_Application.Contracts.DTOs.Articulos;
using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

/// <summary>
/// Comando para crear un nuevo artículo.
/// </summary>
public class CreateArticuloCommand : IRequest<Guid>
{
    public CreateArticuloDto ArticuloDto { get; set; }
}
