using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using System;
using System.Threading.Tasks;

namespace DogWalk_Application.Services;

/// <summary>
/// Servicio para la administración de usuarios y datos del dashboard.
/// </summary>
public class AdminService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Constructor para el servicio de administración.
    /// </summary>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    public AdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Crea un usuario administrador.
    /// </summary>
    /// <param name="createAdminDto">Datos del usuario administrador a crear.</param>
    /// <returns>ID del usuario administrador creado.</returns>
    public async Task<Guid> CreateAdminUser(CreateAdminDto createAdminDto)
    {
        // Verificar si el email ya existe
        var existingUser = await _unitOfWork.Usuarios.GetByEmailAsync(createAdminDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("El email ya está registrado");

        // Crear usuario con rol de administrador
        var admin = new Usuario(
            Guid.NewGuid(),
            Dni.Create(createAdminDto.Dni),
            createAdminDto.Nombre,
            createAdminDto.Apellido,
            Direccion.Create(createAdminDto.Direccion),
            Email.Create(createAdminDto.Email),
            Password.Create(createAdminDto.Password),
            Telefono.Create(createAdminDto.Telefono),
            RolUsuario.Administrador // Asignar rol de administrador
        );

        await _unitOfWork.Usuarios.AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        return admin.Id;
    }

    // Cambiar el rol de un usuario
    public async Task ChangeUserRole(AssignRoleDto assignRoleDto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(assignRoleDto.UserId);
        if (usuario == null)
            throw new InvalidOperationException("Usuario no encontrado");

        RolUsuario nuevoRol;
        if (Enum.TryParse(assignRoleDto.RoleName, true, out nuevoRol))
        {
            // Implementa el cambio de rol aquí
            // usuario.CambiarRol(nuevoRol);
            
            await _unitOfWork.SaveChangesAsync();
        }
        else
            throw new InvalidOperationException("Rol no válido");
    }
    
    // Obtener datos para el dashboard
    public async Task<DashboardSummaryDto> GetDashboardSummary()
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
        var reservas = await _unitOfWork.Reservas.GetAllAsync();
        
        return new DashboardSummaryDto
        {
            TotalUsuarios = usuarios.Count(),
            TotalPaseadores = paseadores.Count(),
            TotalReservas = reservas.Count(),
            ReservasCompletadas = reservas.Count(r => r.Estado == EstadoReserva.Completada),
            ReservasPendientes = reservas.Count(r => r.Estado == EstadoReserva.Pendiente),
            ReservasCanceladas = reservas.Count(r => r.Estado == EstadoReserva.Cancelada),
            PaseadoresMasReservados = paseadores.GroupBy(p => p.Nombre)
                .OrderByDescending(g => g.Count())
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToList()
           
            
        };
    }
}
