using DogWalk_Application.Contracts.DTOs.Auth;
using DogWalk_Application.Contracts.DTOs.Estadisticas;
using DogWalk_Application.Contracts.DTOs.Usuarios;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Registro de usuarios (público)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                // Verificar si el email ya existe
                var existingUser = await _unitOfWork.Usuarios.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                // Verificar que las contraseñas coincidan
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Las contraseñas no coinciden" });
                }

                // Crear usuario con rol normal
                var usuario = new Usuario(
                    Guid.NewGuid(),
                    Dni.Create(registerDto.Dni),
                    registerDto.Nombre,
                    registerDto.Apellido,
                    Direccion.Create(registerDto.Direccion),
                    Email.Create(registerDto.Email),
                    Password.Create(registerDto.Password),
                    Telefono.Create(registerDto.Telefono),
                    RolUsuario.Usuario // Rol por defecto: Usuario normal
                );

                await _unitOfWork.Usuarios.AddAsync(usuario);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new
                {
                    message = "Usuario registrado correctamente",
                    userId = usuario.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al registrar usuario: {ex.Message}" });
            }
        }

        // Obtener perfil de usuario (protegido)
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Obtener perros y reservas
                var perros = await _unitOfWork.Perros.GetByUsuarioIdAsync(userId);
                var reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(userId);

                // Crear DTO de perfil
                var profileDto = new UsuarioProfileDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email.ToString(),
                    FotoPerfil = usuario.FotoPerfil,
                    CantidadPerros = perros?.Count() ?? 0,
                    CantidadReservas = reservas?.Count() ?? 0,
                    Perros = perros?.Select(p => new PerroProfileDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Raza = p.Raza,
                        Edad = p.Edad,
                        GpsUbicacion = p.GpsUbicacion,
                        Fotos = p.Fotos?.Select(f => f.UrlFoto).ToList() ?? new List<string>()
                    }).ToList() ?? new List<PerroProfileDto>()
                };

                return Ok(profileDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener perfil: {ex.Message}" });
            }
        }

        // Actualizar perfil (protegido)
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUsuarioDto updateDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Actualizar datos del usuario
                usuario.ActualizarDatos(
                    updateDto.Nombre,
                    updateDto.Apellido,
                    Direccion.Create(updateDto.Direccion),
                    Telefono.Create(updateDto.Telefono)
                );

                await _unitOfWork.SaveChangesAsync();

                // Obtener datos actualizados incluyendo los perros
                var perros = await _unitOfWork.Perros.GetByUsuarioIdAsync(userId);
                var reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(userId);

                // Devolver el perfil actualizado con todos los datos
                var updatedProfile = new UsuarioProfileDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email.ToString(),
                    FotoPerfil = usuario.FotoPerfil,
                    CantidadPerros = perros?.Count() ?? 0,
                    CantidadReservas = reservas?.Count() ?? 0,
                    Perros = perros?.Select(p => new PerroProfileDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Raza = p.Raza,
                        Edad = p.Edad,
                        GpsUbicacion = p.GpsUbicacion,
                        Fotos = p.Fotos?.Select(f => f.UrlFoto).ToList() ?? new List<string>()
                    }).ToList() ?? new List<PerroProfileDto>()
                };

                return Ok(new
                {
                    message = "Perfil actualizado correctamente",
                    profile = updatedProfile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar perfil: {ex.Message}" });
            }
        }

        // Cambiar contraseña (protegido)
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changeDto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Verificar que las contraseñas nuevas coincidan
                if (changeDto.NewPassword != changeDto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Las contraseñas nuevas no coinciden" });
                }

                // Cambiar contraseña
                try
                {
                    usuario.CambiarPassword(changeDto.CurrentPassword, changeDto.NewPassword);
                }
                catch (InvalidOperationException)
                {
                    return BadRequest(new { message = "La contraseña actual es incorrecta" });
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al cambiar contraseña: {ex.Message}" });
            }
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var usuarioId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                // Obtener datos necesarios
                var reservas = (await _unitOfWork.Reservas.GetByUsuarioIdAsync(usuarioId))?.ToList() ?? new List<Reserva>();
                var perros = (await _unitOfWork.Perros.GetByUsuarioIdAsync(usuarioId))?.ToList() ?? new List<Perro>();
                var facturas = (await _unitOfWork.Facturas.GetByUsuarioIdAsync(usuarioId))?.ToList() ?? new List<Factura>();

                var estadisticas = new EstadisticasUsuarioDto
                {
                    TotalReservas = reservas.Count,
                    TotalGastado = reservas
                        .Where(r => r.Estado == EstadoReserva.Completada && r.Precio != null)
                        .Sum(r => r.Precio?.Cantidad ?? 0),
                    NumeroPerros = perros.Count,
                    ReservasPendientes = reservas.Count(r => r.Estado == EstadoReserva.Pendiente),
                    ReservasCompletadas = reservas.Count(r => r.Estado == EstadoReserva.Completada),

                    // Servicios más usados con validación null
                    ServiciosMasUsados = reservas
                        .Where(r => r.Servicio != null && !string.IsNullOrEmpty(r.Servicio.Nombre))
                        .GroupBy(r => r.Servicio.Nombre)
                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                        .OrderByDescending(x => x.Value)
                        .ToList(),

                    // Paseadores favoritos con validación null
                    PaseadoresFavoritos = reservas
                        .Where(r => r.Paseador != null && !string.IsNullOrEmpty(r.Paseador.Nombre))
                        .GroupBy(r => r.Paseador.Nombre ?? "Desconocido")
                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                        .OrderByDescending(x => x.Value)
                        .ToList()
                };

                // Obtener historial de compras con validación null
                var historialCompras = facturas
                    .Where(f => f != null)
                    .Select(f => new HistorialCompraDto
                    {
                        Id = f.Id,
                        FechaCompra = f.FechaFactura,
                        NumeroFactura = f.Id.ToString(),
                        Total = f.Total?.Cantidad ?? 0,
                        Detalles = f.Detalles?
                            .Where(d => d != null && d.Articulo != null)
                            .Select(d => new DetalleCompraDto
                            {
                                NombreArticulo = d.Articulo?.Nombre ?? "Artículo desconocido",
                                Cantidad = d.Cantidad,
                                PrecioUnitario = d.PrecioUnitario?.Cantidad ?? 0,
                                Subtotal = d.Subtotal?.Cantidad ?? 0
                            })
                            .ToList() ?? new List<DetalleCompraDto>()
                    })
                    .OrderByDescending(h => h.FechaCompra)
                    .ToList();

                return Ok(new
                {
                    Estadisticas = estadisticas,
                    HistorialCompras = historialCompras
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener dashboard: {ex.Message}" });
            }
        }
    }
}
