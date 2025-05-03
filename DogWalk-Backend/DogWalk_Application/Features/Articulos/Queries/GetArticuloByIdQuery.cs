using DogWalk_Application.Contracts.DTOs.Articulos;
using MediatR;
using System;

namespace DogWalk_Application.Features.Articulos.Queries;

public class GetArticuloByIdQuery : IRequest<ArticuloDetailsDto>
{
    public Guid Id { get; set; }
}
