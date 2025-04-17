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
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserManagementDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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