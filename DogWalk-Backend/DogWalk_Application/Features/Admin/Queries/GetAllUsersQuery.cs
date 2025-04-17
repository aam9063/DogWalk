using DogWalk_Application.Contracts.DTOs.Admin;
using MediatR;
using System.Collections.Generic;

namespace DogWalk_Application.Features.Admin.Queries
{
    public record GetAllUsersQuery() : IRequest<List<UserManagementDto>>;
}