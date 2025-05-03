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
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServicioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Obtener todos los servicios (público)
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

        // Método auxiliar para calcular precios de referencia
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

        // Obtener servicio por ID
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

        // Obtener servicios con precios de todos los paseadores
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
        
        // Crear nuevo servicio
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
        
        // Actualizar servicio existente
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
        
        // Eliminar servicio (solo si no está en uso)
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

        // DogWalk_API/Controllers/ServicioController.cs - Agregar este endpoint
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
