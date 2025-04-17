using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Application.Services;
using MediatR;

namespace DogWalk_Application.Features.Admin.Commands
{
    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
    {
        private readonly AdminService _adminService;

        public AssignRoleCommandHandler(AdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            var dto = new AssignRoleDto
            {
                UserId = request.UserId,
                RoleName = request.RoleName
            };

            await _adminService.ChangeUserRole(dto);
        }
    }
}