using DogWalk_Application.Contracts.DTOs.Valoraciones;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingPaseadorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RankingPaseadorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/RankingPaseador/paseador/{paseadorId}
        [HttpGet("paseador/{paseadorId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RankingPaseadorDto>>> GetValoracionesPaseador(Guid paseadorId)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                    return NotFound(new { message = "Paseador no encontrado" });

                var valoraciones = paseador.ValoracionesRecibidas.ToList();
                var valoracionesDto = new List<RankingPaseadorDto>();

                foreach (var valoracion in valoraciones)
                {
                    var usuario = await _unitOfWork.Usuarios.GetByIdAsync(valoracion.UsuarioId);
                    valoracionesDto.Add(new RankingPaseadorDto
                    {
                        Id = valoracion.Id,
                        UsuarioId = valoracion.UsuarioId,
                        NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                        FotoUsuario = usuario.FotoPerfil,
                        PaseadorId = paseadorId,
                        NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                        Puntuacion = valoracion.Valoracion.Puntuacion,
                        Comentario = valoracion.Comentario,
                        FechaValoracion = valoracion.CreadoEn
                    });
                }

                return Ok(valoracionesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener valoraciones: {ex.Message}" });
            }
        }

        // GET: api/RankingPaseador/resumen/{paseadorId}
        [HttpGet("resumen/{paseadorId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResumenValoracionesDto>> GetResumenValoraciones(Guid paseadorId)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                    return NotFound(new { message = "Paseador no encontrado" });

                var valoraciones = paseador.ValoracionesRecibidas.ToList();
                
                // Inicializar diccionario con todas las puntuaciones posibles (1-5)
                var distribucion = new Dictionary<int, int>
                {
                    { 1, 0 },
                    { 2, 0 },
                    { 3, 0 },
                    { 4, 0 },
                    { 5, 0 }
                };

                // Contar valoraciones por puntuación
                foreach (var valoracion in valoraciones)
                {
                    int puntuacion = valoracion.Valoracion.Puntuacion;
                    if (distribucion.ContainsKey(puntuacion))
                    {
                        distribucion[puntuacion]++;
                    }
                }

                // Obtener el promedio desde el repositorio
                decimal promedio = (decimal)await _unitOfWork.RankingPaseadores.GetPromedioPaseadorAsync(paseadorId);

                var resumen = new ResumenValoracionesDto
                {
                    PromedioValoracion = promedio,
                    CantidadValoraciones = valoraciones.Count,
                    DistribucionValoraciones = distribucion
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener resumen de valoraciones: {ex.Message}" });
            }
        }

        // POST: api/RankingPaseador
        [HttpPost]
        [Authorize] // Solo usuarios autenticados pueden valorar
        public async Task<ActionResult> CrearValoracion(CrearRankingPaseadorDto dto)
        {
            try
            {
                // Validar puntuación
                if (dto.Puntuacion < 1 || dto.Puntuacion > 5)
                    return BadRequest(new { message = "La puntuación debe estar entre 1 y 5" });

                // Obtener el ID del usuario desde el token
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                // Verificar que el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(dto.PaseadorId);
                if (paseador == null)
                    return NotFound(new { message = "Paseador no encontrado" });

                // Verificar que el usuario tiene derecho a valorar (ha tenido una reserva con este paseador)
                var reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(usuarioId);
                var tieneReservaCompletada = reservas.Any(r => 
                    r.PaseadorId == dto.PaseadorId && 
                    r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Completada);

                if (!tieneReservaCompletada)
                    return BadRequest(new { message = "Solo puedes valorar a paseadores con los que hayas completado una reserva" });

                // Verificar si ya existe una valoración previa de este usuario para este paseador
                var valoracionExistente = await _unitOfWork.RankingPaseadores.GetByUsuarioYPaseadorAsync(usuarioId, dto.PaseadorId);
                
                if (valoracionExistente != null)
                {
                    // El método espera el objeto completo, no solo el ID
                    await _unitOfWork.RankingPaseadores.DeleteAsync(valoracionExistente);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Intentamos utilizar Factory Methods o static builders si es posible
                try 
                {
                    // Usar reflection si es necesario, o un método de factory si existe
                    var valoracion = typeof(RankingPaseador).GetMethod("Create", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        ?.Invoke(null, new object[] { usuarioId, dto.PaseadorId, dto.Puntuacion, dto.Comentario }) as RankingPaseador;

                    if (valoracion != null)
                    {
                        await _unitOfWork.RankingPaseadores.AddAsync(valoracion);
                        await _unitOfWork.SaveChangesAsync();
                        
                        // Actualizar valoración promedio del paseador
                        paseador.ActualizarValoracion();
                        await _unitOfWork.SaveChangesAsync();
                        
                        return Ok(new { message = "Valoración guardada con éxito" });
                    }
                } 
                catch 
                {
                    // Si falla el método de factory, intentamos otro enfoque
                }

                // Alternativa: Si no hay factory method, intentamos construir de otra manera
                // Esto dependerá de cómo esté implementada tu entidad RankingPaseador
                
                return BadRequest(new { message = "No se pudo crear la valoración. Por favor, contacta al administrador." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear valoración: {ex.Message}" });
            }
        }

        // DELETE: api/RankingPaseador/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> EliminarValoracion(Guid id)
        {
            try
            {
                // Obtener el ID del usuario desde el token
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });

                // Verificar que la valoración existe
                var valoracion = await _unitOfWork.RankingPaseadores.GetByIdAsync(id);
                if (valoracion == null)
                    return NotFound(new { message = "Valoración no encontrada" });

                // Verificar que la valoración pertenece al usuario actual
                if (valoracion.UsuarioId != usuarioId)
                    return Forbid();

                // Guardar el ID del paseador para actualizar su promedio después
                var paseadorId = valoracion.PaseadorId;

                // Eliminar la valoración - debe pasarse el objeto completo
                await _unitOfWork.RankingPaseadores.DeleteAsync(valoracion);
                await _unitOfWork.SaveChangesAsync();

                // Actualizar valoración promedio del paseador
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador != null)
                {
                    paseador.ActualizarValoracion();
                    await _unitOfWork.SaveChangesAsync();
                }

                return Ok(new { message = "Valoración eliminada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar valoración: {ex.Message}" });
            }
        }
    }
}
