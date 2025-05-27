using MediatR;
using System;

namespace DogWalk_Application.Features.Admin.Commands
{
    /// <summary>
    /// Comando para eliminar un usuario.
    /// </summary>
    public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
}