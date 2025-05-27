using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Admin.Commands
{
    /// <summary>
    /// Manejador para crear un nuevo administrador.
    /// </summary>
    public class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de comandos de creación de administrador.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public CreateAdminCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja el comando de creación de administrador.
        /// </summary>
        /// <param name="request">Comando de creación de administrador.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<Guid> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
        {
            // Para simplificar, vamos a intentar crear el admin directamente
            // y dejar que la base de datos maneje errores por email duplicado
            try
            {
                var userId = await _unitOfWork.Usuarios.CreateAdminUserAsync(
                    request.Email,
                    request.Nombre,
                    request.Apellido,
                    request.Telefono,
                    request.Password
                );
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return userId;
            }
            catch (Exception ex)
            {
                // Captura errores y reenvía con un mensaje más descriptivo
                throw new Exception($"Error al crear el administrador: {ex.Message}", ex);
            }
        }
    }
}