using MediatR;
using System;

namespace DogWalk_Application.Features.Admin.Commands
{
    /// <summary>
    /// Comando para crear un nuevo administrador.
    /// </summary>
    public class CreateAdminCommand : IRequest<Guid>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
    }
}