using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Application.Services;
using MediatR;

namespace DogWalk_Application.Features.Admin.Commands
{
    /// <summary>
    /// Manejador para asignar un rol a un usuario.
    /// </summary>
    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand>
    {
        private readonly AdminService _adminService;

        /// <summary>
        /// Constructor para el manejador de comandos de asignación de roles.
        /// </summary>
        /// <param name="adminService">Servicio de administración.</param>
        public AssignRoleCommandHandler(AdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Maneja el comando de asignación de roles.
        /// </summary>
        /// <param name="request">Comando de asignación de roles.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
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