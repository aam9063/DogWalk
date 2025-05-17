using DogWalk_Application.Contracts.DTOs.Auth;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly JwtProvider _jwtProvider;

        public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration, JwtProvider jwtProvider)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _jwtProvider = jwtProvider;
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
                var jwtToken = GenerateJwtToken(user);
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = jwtHandler.ReadJwtToken(jwtToken);

                // Generar refresh token
                var refreshToken = GenerateRefreshToken();

                // Guardar el refresh token en la base de datos
                var token = new RefreshToken(
                    Guid.NewGuid(),
                    user.Id,
                    refreshToken,
                    jwtSecurityToken.Id,
                    DateTime.UtcNow.AddDays(7), // 7 días de validez
                    GetIpAddress(),
                    Request.Headers["User-Agent"].ToString()
                );

                await _unitOfWork.RefreshTokens.AddAsync(token);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Token = jwtToken,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
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

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Solicitud inválida" });

            string accessToken = request.AccessToken;
            string refreshToken = request.RefreshToken;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Token de acceso y refresh token son requeridos" });

            // Validar token expirado
            var principal = _jwtProvider.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return BadRequest(new { message = "Token de acceso inválido" });

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "Token de acceso inválido" });

            // Verificar refresh token en la base de datos
            var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null)
                return BadRequest(new { message = "Refresh token inválido" });

            // Validar propiedades del refresh token
            if (storedRefreshToken.UserId != Guid.Parse(userId) ||
                storedRefreshToken.IsUsed ||
                storedRefreshToken.IsRevoked ||
                storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Refresh token inválido o expirado" });
            }

            // Obtener usuario
            var user = await _unitOfWork.Usuarios.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
                return BadRequest(new { message = "Usuario no encontrado" });

            // Marcar el refresh token actual como usado
            storedRefreshToken.UseToken();

            // Generar nuevos tokens
            var newJwtToken = GenerateJwtToken(user);
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = jwtHandler.ReadJwtToken(newJwtToken);

            var newRefreshToken = GenerateRefreshToken();

            // Guardar el nuevo refresh token
            var token = new RefreshToken(
                Guid.NewGuid(),
                user.Id,
                newRefreshToken,
                jwtSecurityToken.Id,
                DateTime.UtcNow.AddDays(7),
                GetIpAddress(),
                Request.Headers["User-Agent"].ToString()
            );

            await _unitOfWork.RefreshTokens.AddAsync(token);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                Success = true,
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                TokenExpiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                UserId = user.Id,
                Email = user.Email.ToString(),
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Rol = user.Rol.ToString()
            });
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Refresh token es requerido" });

            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);

            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null)
                return NotFound(new { message = "Refresh token no encontrado" });

            if (refreshToken.UserId != userId)
                return Unauthorized(new { message = "No autorizado para revocar este token" });

            if (refreshToken.IsUsed || refreshToken.IsRevoked)
                return BadRequest(new { message = "Token ya usado o revocado" });

            refreshToken.RevokeToken();
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Token revocado exitosamente" });
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

        // Método auxiliar para generar refresh tokens
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        // Método para obtener la dirección IP del cliente
        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "Unknown";
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }
    }
}
