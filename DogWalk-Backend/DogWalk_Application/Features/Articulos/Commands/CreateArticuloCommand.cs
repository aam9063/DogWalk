using DogWalk_Application.Contracts.DTOs.Articulos;
using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Commands;

public class CreateArticuloCommand : IRequest<Guid>
{
    public CreateArticuloDto ArticuloDto { get; set; }
}
