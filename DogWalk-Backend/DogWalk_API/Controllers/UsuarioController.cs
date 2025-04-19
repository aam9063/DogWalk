using DogWalk_Application.Contracts.DTOs.Auth;
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

                return Ok(new { 
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
        [Authorize] // Cualquier usuario autenticado puede ver su perfil
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Obtener ID del usuario desde el token
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Contar perros y reservas
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
                    CantidadReservas = reservas?.Count() ?? 0
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

                // Actualizar datos
                usuario.ActualizarDatos(
                    updateDto.Nombre,
                    updateDto.Apellido,
                    Direccion.Create(updateDto.Direccion),
                    Telefono.Create(updateDto.Telefono)
                );

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Perfil actualizado correctamente" });
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
    }
}
