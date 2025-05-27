using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

/// <summary>
/// Comando para actualizar el stock de un artículo.
/// </summary>
public class UpdateArticuloStockCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public int Cantidad { get; set; }
}
