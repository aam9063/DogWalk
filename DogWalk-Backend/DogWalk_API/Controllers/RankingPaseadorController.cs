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
using Microsoft.Extensions.Logging;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con las valoraciones de paseadores.
    /// Incluye la gestión de puntuaciones, comentarios y estadísticas de valoración.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RankingPaseadorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del controlador de valoraciones.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        public RankingPaseadorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todas las valoraciones de un paseador específico.
        /// </summary>
        /// <param name="paseadorId">ID del paseador</param>
        /// <returns>Lista de valoraciones del paseador</returns>
        /// <response code="200">Retorna la lista de valoraciones</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
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

        /// <summary>
        /// Obtiene un resumen estadístico de las valoraciones de un paseador.
        /// </summary>
        /// <param name="paseadorId">ID del paseador</param>
        /// <returns>Resumen estadístico de valoraciones</returns>
        /// <response code="200">Retorna el resumen de valoraciones</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
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

        /// <summary>
        /// Crea una nueva valoración para un paseador.
        /// </summary>
        /// <param name="dto">Datos de la valoración</param>
        /// <returns>Confirmación de la creación</returns>
        /// <response code="200">Si la valoración se creó correctamente</response>
        /// <response code="400">Si los datos son inválidos o el usuario no puede valorar al paseador</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CrearValoracion(CrearRankingPaseadorDto dto)
        {
            try
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RankingPaseadorController>>();
                
                // Cambiar el claim que buscamos
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => 
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                    
                if (string.IsNullOrEmpty(usuarioIdClaim))
                {
                    logger.LogError("No se encontró el claim de identificación del usuario");
                    return Unauthorized(new { message = "No se pudo identificar al usuario" });
                }

                if (!Guid.TryParse(usuarioIdClaim, out Guid usuarioId))
                {
                    logger.LogError("El ID del usuario no es un GUID válido: {UsuarioIdClaim}", usuarioIdClaim);
                    return Unauthorized(new { message = "Token inválido: formato de ID incorrecto" });
                }

                // Validar puntuación
                if (dto.Puntuacion < 1 || dto.Puntuacion > 5)
                    return BadRequest(new { message = "La puntuación debe estar entre 1 y 5" });

                // Verificar que el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(dto.PaseadorId);
                if (paseador == null)
                    return NotFound(new { message = "Paseador no encontrado" });

                // Verificar que el usuario tiene derecho a valorar
                var reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(usuarioId);
                var tieneReservaCompletada = reservas.Any(r => 
                    r.PaseadorId == dto.PaseadorId && 
                    r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Completada);

                if (!tieneReservaCompletada)
                    return BadRequest(new { message = "Solo puedes valorar a paseadores con los que hayas completado una reserva" });

                // Verificar si ya existe una valoración previa
                var valoracionExistente = await _unitOfWork.RankingPaseadores.GetByUsuarioYPaseadorAsync(usuarioId, dto.PaseadorId);
                
                if (valoracionExistente != null)
                {
                    await _unitOfWork.RankingPaseadores.DeleteAsync(valoracionExistente);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Primero, necesitamos crear el objeto Valoracion usando el método factory
                var valoracion = Valoracion.Create(dto.Puntuacion);

                // Luego crear el RankingPaseador con los tipos correctos
                var rankingPaseador = new RankingPaseador(
                    Guid.NewGuid(),  // Id
                    usuarioId,       // UsuarioId
                    dto.PaseadorId,  // PaseadorId
                    valoracion,      // Valoracion (Value Object)
                    dto.Comentario   // Comentario
                );

                await _unitOfWork.RankingPaseadores.AddAsync(rankingPaseador);
                await _unitOfWork.SaveChangesAsync();
                
                // Actualizar valoración promedio del paseador
                paseador.ActualizarValoracion();
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { message = "Valoración guardada con éxito" });
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RankingPaseadorController>>();
                logger.LogError(ex, "Error al crear valoración");
                return StatusCode(500, new { message = $"Error al crear valoración: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina una valoración existente.
        /// </summary>
        /// <param name="id">ID de la valoración a eliminar</param>
        /// <returns>Confirmación de la eliminación</returns>
        /// <response code="200">Si la valoración se eliminó correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no tiene permiso para eliminar esta valoración</response>
        /// <response code="404">Si la valoración no se encuentra</response>
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
