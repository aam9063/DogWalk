using DogWalk_Application.Contracts.DTOs.Auth;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Obtener el usuario por email
                var user = await _unitOfWork.Usuarios.GetByEmailAsync(loginDto.Email);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }
                
                // Verificar la contraseña utilizando el método Verify
                bool passwordValid = false;
                try 
                {
                    passwordValid = user.Password.Verify(loginDto.Password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al verificar contraseña: {ex.Message}");
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }
                
                if (!passwordValid)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }
                
                // Generar token JWT
                var token = GenerateJwtToken(user);
                
                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email.ToString(),
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Rol = user.Rol.ToString()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("login-direct")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginDirect([FromBody] LoginDto loginDto)
        {
            // Obtener usuario por email
            var user = await _unitOfWork.Usuarios.GetByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email o contraseña incorrectos" });
            }

            Console.WriteLine($"Usuario encontrado: {user.Id}, Nombre: {user.Nombre}");

            // IMPORTANTE: Para propósitos de prueba, siempre autenticamos si el usuario existe
            // ESTO ES TEMPORAL - SOLO PARA DESARROLLO/PRUEBAS
            // En producción, siempre debes verificar la contraseña correctamente

            // Generar token JWT sin verificar la contraseña
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                token = token,
                userId = user.Id,
                email = user.Email.ToString(),
                nombre = user.Nombre,
                apellido = user.Apellido,
                rol = user.Rol.ToString()
            });
        }

        [HttpPost("test-password")]
        [AllowAnonymous]
        public async Task<IActionResult> TestPassword([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _unitOfWork.Usuarios.GetByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Intentar verificar la contraseña
                bool passwordValid = false;
                string errorMessage = null;

                try
                {
                    passwordValid = user.Password.Verify(loginDto.Password);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }

                return Ok(new
                {
                    userId = user.Id,
                    email = user.Email.ToString(),
                    passwordProvided = loginDto.Password,
                    passwordValid,
                    errorMessage,
                    passwordType = user.Password?.GetType().Name,
                    // No incluir información sensible como passwordHash en producción
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error en la verificación: {ex.Message}" });
            }
        }

        [HttpPost("admin/reset-password")]
        [AllowAnonymous] // Temporal para poder reiniciar contraseñas sin autenticación
        public async Task<IActionResult> AdminResetPassword([FromBody] ResetAdminPasswordDto resetDto)
        {
            try
            {
                // Verificar la clave de seguridad (puedes cambiarla por una constante más segura)
                if (resetDto.SecurityKey != "ResetPasswordKey123")
                {
                    return Unauthorized(new { message = "Clave de seguridad incorrecta" });
                }
                
                // Obtener el usuario por ID
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(resetDto.UserId);
                
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }
                
                // Actualizar la contraseña
                await _unitOfWork.Usuarios.UpdatePasswordAsync(resetDto.UserId, resetDto.NewPassword);
                
                return Ok(new { 
                    message = "Contraseña actualizada correctamente", 
                    userId = usuario.Id,
                    email = usuario.Email.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al reiniciar la contraseña: {ex.Message}" });
            }
        }

        private string GenerateJwtToken(DogWalk_Domain.Entities.Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email.ToString()),
                new Claim(ClaimTypes.Role, user.Rol.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
