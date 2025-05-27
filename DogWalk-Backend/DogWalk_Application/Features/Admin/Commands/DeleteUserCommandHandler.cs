using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using DogWalk_Domain.Common.Enums;

namespace DogWalk_Application.Features.Admin.Commands
{
    /// <summary>
    /// Manejador para eliminar un usuario.
    /// </summary>
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de comandos de eliminación de usuario.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja el comando de eliminación de usuario.
        /// </summary>
        /// <param name="request">Comando de eliminación de usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UserId);
            
            if (usuario == null)
                return false;

            // Verificar que no sea un administrador
            if (usuario.Rol == RolUsuario.Administrador)
                throw new InvalidOperationException("No se puede eliminar un usuario administrador");

            // Verificar si el usuario tiene reservas activas
            var reservasActivas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(request.UserId);
            if (reservasActivas.Any(r => r.Estado == EstadoReserva.Pendiente))
                throw new InvalidOperationException("No se puede eliminar un usuario con reservas pendientes");

            // Eliminar el usuario
            await _unitOfWork.Usuarios.DeleteAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}