using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DogWalk_Application.Contracts.DTOs.Servicios;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con los servicios de paseo.
    /// Incluye gestión de tipos de servicios y sus precios de referencia.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del controlador de servicios.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        public ServicioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los servicios disponibles.
        /// </summary>
        /// <returns>Lista de servicios con sus precios de referencia</returns>
        /// <response code="200">Retorna la lista de servicios</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicios()
        {
            try
            {
                var servicios = await _unitOfWork.Servicios.GetAllAsync();
                
                var serviciosDto = servicios.Select(s => new
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Descripcion = s.Descripcion,
                    Tipo = s.Tipo.ToString(),
                    PrecioReferencia = ObtenerPrecioReferencia(s.Tipo.ToString())
                }).ToList();
                
                return Ok(serviciosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener servicios: {ex.Message}" });
            }
        }

        /// <summary>
        /// Calcula el precio de referencia para un tipo de servicio.
        /// </summary>
        /// <param name="tipoServicio">Tipo de servicio</param>
        /// <returns>Precio de referencia calculado</returns>
        private decimal ObtenerPrecioReferencia(string tipoServicio)
        {
            return tipoServicio switch
            {
                "Paseo" => 10.0m,
                "GuarderiaDia" => 25.0m,
                "GuarderiaNoche" => 35.0m,
                _ => 15.0m
            };
        }

        /// <summary>
        /// Obtiene los detalles de un servicio específico.
        /// </summary>
        /// <param name="id">ID del servicio</param>
        /// <returns>Detalles del servicio</returns>
        /// <response code="200">Retorna los detalles del servicio</response>
        /// <response code="404">Si el servicio no se encuentra</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicioById(Guid id)
        {
            try
            {
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
                if (servicio == null)
                {
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                
                var servicioDto = new
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    Tipo = servicio.Tipo.ToString(),
                    PrecioReferencia = ObtenerPrecioReferencia(servicio.Tipo.ToString())
                };
                
                return Ok(servicioDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener servicio: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene todos los servicios con sus precios por paseador.
        /// </summary>
        /// <returns>Lista de servicios con estadísticas de precios</returns>
        /// <response code="200">Retorna la lista de servicios con precios</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpGet("with-precios")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiciosWithPrecios()
        {
            try
            {
                var servicios = await _unitOfWork.Servicios.GetAllAsync();
                var result = new List<ServicioWithPreciosDto>();
                
                foreach (var servicio in servicios)
                {
                    // Obtener todos los precios para este servicio
                    var precios = servicio.Precios.ToList();
                    
                    // Calcular estadísticas
                    decimal precioMinimo = precios.Any() ? precios.Min(p => p.Valor.Cantidad) : 0;
                    decimal precioMaximo = precios.Any() ? precios.Max(p => p.Valor.Cantidad) : 0;
                    decimal precioPromedio = precios.Any() ? precios.Average(p => p.Valor.Cantidad) : 0;
                    
                    // Crear DTO con precios
                    var servicioDto = new ServicioWithPreciosDto
                    {
                        Id = servicio.Id,
                        Nombre = servicio.Nombre,
                        Descripcion = servicio.Descripcion,
                        Tipo = servicio.Tipo.ToString(),
                        PrecioMinimo = precioMinimo,
                        PrecioMaximo = precioMaximo,
                        PrecioPromedio = Math.Round(precioPromedio, 2),
                        Precios = precios.Select(p => new PrecioPaseadorDto
                        {
                            PaseadorId = p.PaseadorId,
                            NombrePaseador = $"{p.Paseador.Nombre} {p.Paseador.Apellido}",
                            FotoPaseador = p.Paseador.FotoPerfil,
                            Precio = p.Valor.Cantidad,
                            ValoracionPaseador = p.Paseador.ValoracionGeneral
                        }).ToList()
                    };
                    
                    result.Add(servicioDto);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener servicios con precios: {ex.Message}" });
            }
        }

        // Endpoints de administración (solo para administradores)
        
        /// <summary>
        /// Crea un nuevo servicio (solo administradores).
        /// </summary>
        /// <param name="createDto">Datos del nuevo servicio</param>
        /// <returns>Detalles del servicio creado</returns>
        /// <response code="201">Si el servicio se creó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es administrador</response>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateServicio([FromBody] CreateServicioDto createDto)
        {
            try
            {
                // Convertir string a TipoServicio
                if (!Enum.TryParse<TipoServicio>(createDto.Tipo, out var tipoServicio))
                {
                    return BadRequest(new { message = "Tipo de servicio no válido" });
                }
                
                var servicio = new Servicio(
                    Guid.NewGuid(),
                    createDto.Nombre,
                    createDto.Descripcion,
                    tipoServicio
                );
                
                await _unitOfWork.Servicios.AddAsync(servicio);
                await _unitOfWork.SaveChangesAsync();
                
                return CreatedAtAction(nameof(GetServicioById), new { id = servicio.Id }, new ServicioDto
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    Tipo = servicio.Tipo.ToString(),
                    PrecioReferencia = createDto.PrecioReferencia
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear servicio: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Actualiza un servicio existente (solo administradores).
        /// </summary>
        /// <param name="id">ID del servicio</param>
        /// <param name="updateDto">Datos actualizados del servicio</param>
        /// <returns>Confirmación de la actualización</returns>
        /// <response code="200">Si el servicio se actualizó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es administrador</response>
        /// <response code="404">Si el servicio no se encuentra</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateServicio(Guid id, [FromBody] UpdateServicioDto updateDto)
        {
            try
            {
                if (id != updateDto.Id)
                {
                    return BadRequest(new { message = "ID de servicio no coincide" });
                }
                
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
                if (servicio == null)
                {
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                
                // Actualizar servicio
                servicio.ActualizarDatos(
                    updateDto.Nombre,
                    updateDto.Descripcion,
                    servicio.Tipo // Mantener el tipo original, no permitir cambios
                );
                
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new
                {
                    Id = servicio.Id,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    Tipo = servicio.Tipo.ToString(),
                    PrecioReferencia = updateDto.PrecioReferencia
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar servicio: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Elimina un servicio existente (solo administradores).
        /// </summary>
        /// <param name="id">ID del servicio a eliminar</param>
        /// <returns>Confirmación de la eliminación</returns>
        /// <response code="200">Si el servicio se eliminó correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="403">Si el usuario no es administrador</response>
        /// <response code="404">Si el servicio no se encuentra</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteServicio(Guid id)
        {
            try
            {
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(id);
                if (servicio == null)
                {
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                
                // Verificar si el servicio tiene precios o reservas
                if (servicio.Precios.Any() || servicio.Reservas.Any())
                {
                    return BadRequest(new { message = "No se puede eliminar un servicio en uso" });
                }
                
                await _unitOfWork.Servicios.DeleteAsync(servicio);
                await _unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar servicio: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene los filtros disponibles para servicios.
        /// </summary>
        /// <returns>Lista de filtros y opciones disponibles</returns>
        /// <response code="200">Retorna los filtros disponibles</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
        [HttpGet("filtros")]
        [AllowAnonymous]
        public async Task<ActionResult> GetServiciosFiltros()
        {
            try
            {
                var servicios = await _unitOfWork.Servicios.GetAllAsync();
                
                var serviciosFiltros = servicios.Select(s => new
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Tipo = s.Tipo.ToString()
                }).ToList();
                
                return Ok(serviciosFiltros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener servicios: {ex.Message}" });
            }
        }
    }
}
