using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

public class DeleteArticuloCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
