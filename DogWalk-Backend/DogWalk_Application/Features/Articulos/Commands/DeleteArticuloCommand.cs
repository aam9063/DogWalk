using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

/// <summary>
/// Comando para eliminar un artículo.
/// </summary>
public class DeleteArticuloCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
