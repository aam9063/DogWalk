using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Admin.Queries
{
    /// <summary>
    /// Manejador para obtener todos los usuarios.
    /// </summary>
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserManagementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de consultas de usuarios.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja la consulta para obtener todos los usuarios.
        /// </summary>
        /// <param name="request">Consulta para obtener todos los usuarios.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<List<UserManagementDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();

            return usuarios.Select(u => new UserManagementDto
            {
                Id = u.Id,
                Dni = u.Dni?.ToString() ?? string.Empty,
                NombreCompleto = $"{u.Nombre ?? ""} {u.Apellido ?? ""}".Trim(),
                Email = u.Email?.ToString() ?? string.Empty,
                Telefono = u.Telefono?.ToString() ?? string.Empty,
                Rol = u.Rol.ToString(),
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                TotalPerros = u.Perros?.Count ?? 0,
                TotalReservas = u.Reservas?.Count ?? 0,
                TotalFacturado = 0m,
                ValoracionPromedio = null,
                TotalValoraciones = null,
                Bloqueado = false,
                FechaUltimoAcceso = null,
                AccionesDisponibles = new List<string> { "ver", "editar", "bloquear" }
            }).ToList();
        }
    }
}