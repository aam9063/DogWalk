using MediatR;
using System;

namespace DogWalk_Application.Features.Admin.Commands
{
    public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
}