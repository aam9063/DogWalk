using MediatR;

namespace DogWalk_Application.Features.Admin.Commands
{
    public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest;
}




   