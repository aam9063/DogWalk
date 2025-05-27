using DogWalk_Application.Contracts.DTOs.Disponibilidad;
using DogWalk_Domain.Common.Enums;
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
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con la disponibilidad horaria de los paseadores.
    /// Incluye gestión de horarios, slots de tiempo y estados de disponibilidad.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibilidadHorariaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del controlador de disponibilidad horaria.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        public DisponibilidadHorariaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene la disponibilidad horaria de un paseador específico.
        /// </summary>
        /// <param name="paseadorId">ID del paseador</param>
        /// <param name="fecha">Fecha opcional para filtrar la disponibilidad</param>
        /// <returns>Disponibilidad horaria agrupada por días</returns>
        /// <response code="200">Retorna la disponibilidad del paseador</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpGet("paseador/{paseadorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDisponibilidadPaseador(Guid paseadorId, [FromQuery] DateTime? fecha = null)
        {
            try
            {
                // Verificar que el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                // Obtener disponibilidad
                IEnumerable<DisponibilidadHoraria> disponibilidades;
                if (fecha.HasValue)
                {
                    // Si hay fecha, obtener solo para esa fecha
                    DateTime fechaInicio = fecha.Value.Date;
                    DateTime fechaFin = fechaInicio.AddDays(1).AddSeconds(-1);
                    disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorYFechaAsync(paseadorId, fechaInicio);
                }
                else
                {
                    // Si no hay fecha, obtener toda la disponibilidad futura
                    disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorIdAsync(paseadorId);
                    disponibilidades = disponibilidades.Where(d => d.FechaHora >= DateTime.Now);
                }

                // Agrupar por días
                var agrupadas = disponibilidades
                    .OrderBy(d => d.FechaHora)
                    .GroupBy(d => d.FechaHora.Date)
                    .Select(grupo => new DisponibilidadDiaDto
                    {
                        Fecha = grupo.Key,
                        Horas = grupo.Select(d => new DisponibilidadHoraDto
                        {
                            Id = d.Id,
                            FechaHora = d.FechaHora,
                            Estado = d.Estado.ToString()
                        }).ToList()
                    })
                    .ToList();

                var result = new DisponibilidadPaseadorDto
                {
                    PaseadorId = paseadorId,
                    NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                    Dias = agrupadas
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener disponibilidad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene la disponibilidad horaria del paseador autenticado.
        /// </summary>
        /// <param name="fecha">Fecha opcional para filtrar la disponibilidad</param>
        /// <returns>Lista de slots de disponibilidad</returns>
        /// <response code="200">Retorna la disponibilidad del paseador</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es un paseador</response>
        /// <response code="404">Si el perfil de paseador no se encuentra</response>
        [HttpGet("mis-horarios")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> GetMiDisponibilidad([FromQuery] DateTime? fecha = null)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Verificar que el usuario es un paseador
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Perfil de paseador no encontrado" });
                }

                // Obtener disponibilidad
                IEnumerable<DisponibilidadHoraria> disponibilidades;
                if (fecha.HasValue)
                {
                    // Si hay fecha, obtener solo para esa fecha
                    DateTime fechaInicio = fecha.Value.Date;
                    disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorYFechaAsync(paseadorId, fechaInicio);
                }
                else
                {
                    // Si no hay fecha, obtener toda la disponibilidad
                    disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorIdAsync(paseadorId);
                }

                // Convertir a DTOs
                var disponibilidadesDto = disponibilidades
                    .OrderBy(d => d.FechaHora)
                    .Select(d => new DisponibilidadHorariaDto
                    {
                        Id = d.Id,
                        PaseadorId = d.PaseadorId,
                        FechaHora = d.FechaHora,
                        Estado = d.Estado.ToString()
                    })
                    .ToList();

                return Ok(disponibilidadesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener disponibilidad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea nuevos slots de disponibilidad horaria para el paseador autenticado.
        /// </summary>
        /// <param name="dto">Datos para crear los slots de disponibilidad</param>
        /// <returns>Lista de slots de disponibilidad creados</returns>
        /// <response code="200">Si los slots se crearon correctamente</response>
        /// <response code="400">Si los datos son inválidos o las fechas no son correctas</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es un paseador</response>
        /// <response code="404">Si el perfil de paseador no se encuentra</response>
        [HttpPost]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> CrearDisponibilidad([FromBody] CrearDisponibilidadDto dto)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Verificar que el usuario es un paseador
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Perfil de paseador no encontrado" });
                }

                // Validar fechas
                if (dto.FechaHoraInicio >= dto.FechaHoraFin)
                {
                    return BadRequest(new { message = "La fecha de inicio debe ser anterior a la fecha de fin" });
                }

                if (dto.FechaHoraInicio < DateTime.Now)
                {
                    return BadRequest(new { message = "La fecha de inicio debe ser futura" });
                }

                if (dto.IntervaloMinutos <= 0)
                {
                    return BadRequest(new { message = "El intervalo debe ser mayor a 0 minutos" });
                }

                // Generar slots de disponibilidad
                var fechaActual = dto.FechaHoraInicio;
                var nuevasDisponibilidades = new List<DisponibilidadHoraria>();

                while (fechaActual < dto.FechaHoraFin)
                {
                    // Verificar si ya existe disponibilidad para esta fecha y hora
                    bool existeDisponibilidad = await _unitOfWork.DisponibilidadHoraria.ExisteDisponibilidad(paseadorId, fechaActual);
                    
                    if (!existeDisponibilidad)
                    {
                        var nuevaDisponibilidad = new DisponibilidadHoraria(
                            Guid.NewGuid(),
                            paseadorId,
                            fechaActual,
                            EstadoDisponibilidad.Disponible
                        );
                        
                        nuevasDisponibilidades.Add(nuevaDisponibilidad);
                        await _unitOfWork.DisponibilidadHoraria.AddAsync(nuevaDisponibilidad);
                    }

                    fechaActual = fechaActual.AddMinutes(dto.IntervaloMinutos);
                }

                await _unitOfWork.SaveChangesAsync();

                // Convertir a DTOs para respuesta
                var disponibilidadesDto = nuevasDisponibilidades
                    .Select(d => new DisponibilidadHorariaDto
                    {
                        Id = d.Id,
                        PaseadorId = d.PaseadorId,
                        FechaHora = d.FechaHora,
                        Estado = d.Estado.ToString()
                    })
                    .ToList();

                return Ok(new { 
                    message = $"Se crearon {disponibilidadesDto.Count} horarios de disponibilidad", 
                    disponibilidades = disponibilidadesDto 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear disponibilidad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza el estado de un slot de disponibilidad horaria.
        /// </summary>
        /// <param name="dto">Datos para actualizar la disponibilidad</param>
        /// <returns>Confirmación de la actualización</returns>
        /// <response code="200">Si el slot se actualizó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es un paseador</response>
        /// <response code="404">Si el slot no se encuentra</response>
        [HttpPut]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> ActualizarDisponibilidad([FromBody] ActualizarDisponibilidadDto dto)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Obtener la disponibilidad
                var disponibilidad = await _unitOfWork.DisponibilidadHoraria.GetByIdAsync(dto.DisponibilidadId);
                if (disponibilidad == null)
                {
                    return NotFound(new { message = "Disponibilidad no encontrada" });
                }

                // Verificar que pertenece al paseador
                if (disponibilidad.PaseadorId != paseadorId && !User.IsInRole("Administrador"))
                {
                    return Forbid();
                }

                // Verificar que la disponibilidad es futura
                if (disponibilidad.FechaHora < DateTime.Now)
                {
                    return BadRequest(new { message = "No se puede modificar disponibilidad pasada" });
                }

                // Verificar si tiene reservas asociadas
                var reservasAsociadas = await _unitOfWork.Reservas.GetAllAsync(); // Aquí debería haber un método más específico
                var tieneReservas = reservasAsociadas.Any(r => 
                    r.DisponibilidadId == disponibilidad.Id && 
                    r.Estado != EstadoReserva.Cancelada);

                if (tieneReservas && dto.Estado == "Disponible" && disponibilidad.Estado == EstadoDisponibilidad.Reservado)
                {
                    return BadRequest(new { message = "No se puede cambiar a disponible un horario con reservas activas" });
                }

                // Cambiar estado
                EstadoDisponibilidad nuevoEstado;
                if (Enum.TryParse(dto.Estado, out nuevoEstado))
                {
                    disponibilidad.CambiarEstado(nuevoEstado);
                    await _unitOfWork.SaveChangesAsync();

                    return Ok(new
                    {
                        Id = disponibilidad.Id,
                        PaseadorId = disponibilidad.PaseadorId,
                        FechaHora = disponibilidad.FechaHora,
                        Estado = disponibilidad.Estado.ToString()
                    });
                }
                else
                {
                    return BadRequest(new { message = "Estado no válido" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar disponibilidad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina un slot de disponibilidad horaria.
        /// </summary>
        /// <param name="id">ID del slot a eliminar</param>
        /// <returns>Confirmación de la eliminación</returns>
        /// <response code="200">Si el slot se eliminó correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es un paseador</response>
        /// <response code="404">Si el slot no se encuentra</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> EliminarDisponibilidad(Guid id)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Obtener la disponibilidad
                var disponibilidad = await _unitOfWork.DisponibilidadHoraria.GetByIdAsync(id);
                if (disponibilidad == null)
                {
                    return NotFound(new { message = "Disponibilidad no encontrada" });
                }

                // Verificar que pertenece al paseador
                if (disponibilidad.PaseadorId != paseadorId && !User.IsInRole("Administrador"))
                {
                    return Forbid();
                }

                // Verificar que la disponibilidad es futura
                if (disponibilidad.FechaHora < DateTime.Now)
                {
                    return BadRequest(new { message = "No se puede eliminar disponibilidad pasada" });
                }

                // Verificar si tiene reservas asociadas
                var reservasAsociadas = await _unitOfWork.Reservas.GetAllAsync(); // Aquí debería haber un método más específico
                var tieneReservas = reservasAsociadas.Any(r => 
                    r.DisponibilidadId == disponibilidad.Id && 
                    r.Estado != EstadoReserva.Cancelada);

                if (tieneReservas)
                {
                    return BadRequest(new { message = "No se puede eliminar un horario con reservas activas" });
                }

                // Eliminar disponibilidad
                await _unitOfWork.DisponibilidadHoraria.DeleteAsync(disponibilidad);
                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar disponibilidad: {ex.Message}" });
            }
        }
    }
}
