using MediatR;

namespace DogWalk_Application.Features.Admin.Commands
{
    /// <summary>
    /// Comando para asignar un rol a un usuario.
    /// </summary>
    public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest;
}




   