using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

public class UpdateArticuloStockCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public int Cantidad { get; set; }
}
