using DogWalk_Application.Contracts.DTOs.Reservas;
using DogWalk_Domain.Common.Enums;
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
    [Route("api/[controller]")]
    [ApiController]
    public class ReservaController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReservaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Obtener todas las reservas del usuario autenticado
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetReservas()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                IEnumerable<Reserva> reservas;
                
                if (User.IsInRole("Paseador"))
                {
                    // Si es paseador, obtener sus reservas como paseador
                    reservas = await _unitOfWork.Reservas.GetByPaseadorIdAsync(userId);
                }
                else
                {
                    // Si es usuario normal, obtener sus reservas como cliente
                    reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(userId);
                }

                var reservasDto = await MapReservasADtos(reservas);
                
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener reservas: {ex.Message}" });
            }
        }

        // Obtener una reserva por ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReservaById(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                
                if (reserva == null)
                {
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                // Verificar que la reserva pertenece al usuario o al paseador
                bool esPropietario = reserva.UsuarioId == userId || reserva.PaseadorId == userId;
                bool esAdmin = User.IsInRole("Administrador");
                
                if (!esPropietario && !esAdmin)
                {
                    return Forbid();
                }
                
                // Cargar entidades relacionadas
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(reserva.UsuarioId);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId);
                var perro = await _unitOfWork.Perros.GetByIdAsync(reserva.PerroId);
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(reserva.ServicioId);
                
                // Usar el método auxiliar en lugar de declarar una variable
                var fechaServicio = await ObtenerFechaServicioAsync(reserva.DisponibilidadId);
                
                // Determinar las acciones permitidas
                bool puedeEditar = reserva.Estado == EstadoReserva.Pendiente;
                bool puedeCancelar = reserva.Estado != EstadoReserva.Cancelada && reserva.Estado != EstadoReserva.Completada;
                bool puedeCompletar = reserva.Estado == EstadoReserva.Confirmada && reserva.PaseadorId == userId;
                bool puedeValorar = reserva.Estado == EstadoReserva.Completada && reserva.UsuarioId == userId;
                
                // Crear DTO detallado
                var reservaDto = new ReservaDetailsDto
                {
                    Id = reserva.Id,
                    FechaReserva = reserva.FechaReserva,
                    FechaServicio = fechaServicio,
                    Estado = reserva.Estado.ToString(),
                    Precio = reserva.Precio.Cantidad,
                    DireccionRecogida = "Dirección de recogida", // Esto debe venir de otro lado
                    DireccionEntrega = "Dirección de entrega", // Esto debe venir de otro lado
                    Notas = "Notas de la reserva", // Esto debe venir de otro lado
                    PuedeEditar = puedeEditar,
                    PuedeCancelar = puedeCancelar,
                    PuedeCompletar = puedeCompletar,
                    PuedeValorar = puedeValorar,
                    
                    // Información relacionada
                    Usuario = new() { 
                        Id = usuario.Id, 
                        Nombre = usuario.Nombre, 
                        Apellido = usuario.Apellido,
                        Email = usuario.Email.ToString()
                    },
                    Paseador = new() { 
                        Id = paseador.Id, 
                        Nombre = paseador.Nombre, 
                        Apellido = paseador.Apellido, 
                        Telefono = paseador.Telefono.ToString() 
                    },
                    Perro = new() { 
                        Id = perro.Id, 
                        Nombre = perro.Nombre, 
                        Raza = perro.Raza, 
                        Edad = perro.Edad 
                    },
                    Servicio = new() { 
                        Id = servicio.Id, 
                        Nombre = servicio.Nombre, 
                        Descripcion = servicio.Descripcion
                    }
                };
                
                return Ok(reservaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener reserva: {ex.Message}" });
            }
        }

        // Crear una nueva reserva
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReserva([FromBody] CreateReservaDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Verificar que el perro existe y pertenece al usuario
                var perro = await _unitOfWork.Perros.GetByIdAsync(dto.PerroId);
                if (perro == null)
                {
                    return NotFound(new { message = "Perro no encontrado" });
                }
                
                if (perro.UsuarioId != userId && !User.IsInRole("Administrador"))
                {
                    return Forbid();
                }
                
                // Verificar que el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(dto.PaseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }
                
                // Verificar que el servicio existe
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(dto.ServicioId);
                if (servicio == null)
                {
                    return NotFound(new { message = "Servicio no encontrado" });
                }
                
                // Buscar disponibilidad para la fecha y hora solicitada
                var disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorYFechaAsync(dto.PaseadorId, dto.FechaServicio);
                var disponibilidad = disponibilidades.FirstOrDefault(d => 
                    d.FechaHora == dto.FechaServicio && 
                    d.Estado == EstadoDisponibilidad.Disponible);
                
                if (disponibilidad == null)
                {
                    return BadRequest(new { message = "No hay disponibilidad para la fecha y hora seleccionada" });
                }
                
                // Obtener el precio del servicio para este paseador
                var precioServicio = paseador.Precios.FirstOrDefault(p => p.ServicioId == dto.ServicioId);
                if (precioServicio == null)
                {
                    return BadRequest(new { message = "El paseador no ofrece este servicio" });
                }
                
                // Crear la reserva
                var nuevaReserva = new Reserva(
                    Guid.NewGuid(),
                    userId,
                    dto.PaseadorId,
                    dto.ServicioId,
                    dto.PerroId,
                    disponibilidad.Id,
                    DateTime.Now,
                    precioServicio.Valor,
                    EstadoReserva.Pendiente
                );
                
                // Actualizar estado de la disponibilidad
                disponibilidad.CambiarEstado(EstadoDisponibilidad.Reservado);
                
                // Guardar cambios
                await _unitOfWork.Reservas.AddAsync(nuevaReserva);
                await _unitOfWork.SaveChangesAsync();
                
                // Obtener datos para respuesta
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
                
                var reservaDto = new ReservaDto
                {
                    Id = nuevaReserva.Id,
                    UsuarioId = nuevaReserva.UsuarioId,
                    NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                    PaseadorId = nuevaReserva.PaseadorId,
                    NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                    PerroId = nuevaReserva.PerroId,
                    NombrePerro = perro.Nombre,
                    ServicioId = nuevaReserva.ServicioId,
                    NombreServicio = servicio.Nombre,
                    FechaReserva = nuevaReserva.FechaReserva,
                    FechaServicio = disponibilidad.FechaHora,
                    Estado = nuevaReserva.Estado.ToString(),
                    Precio = nuevaReserva.Precio.Cantidad,
                    DireccionRecogida = dto.DireccionRecogida,
                    DireccionEntrega = dto.DireccionEntrega
                };
                
                return CreatedAtAction(nameof(GetReservaById), new { id = nuevaReserva.Id }, reservaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear reserva: {ex.Message}" });
            }
        }

        // Actualizar estado de reserva
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateReserva([FromBody] UpdateReservaDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(dto.Id);
                
                if (reserva == null)
                {
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                // Verificar permisos
                bool esPropietario = reserva.UsuarioId == userId || reserva.PaseadorId == userId;
                bool esAdmin = User.IsInRole("Administrador");
                
                if (!esPropietario && !esAdmin)
                {
                    return Forbid();
                }
                
                // Cambiar estado si se proporciona
                if (!string.IsNullOrEmpty(dto.Estado))
                {
                    EstadoReserva nuevoEstado;
                    if (Enum.TryParse(dto.Estado, out nuevoEstado))
                    {
                        // Validar cambios de estado permitidos
                        if (nuevoEstado == EstadoReserva.Cancelada)
                        {
                            // Cualquiera puede cancelar una reserva pendiente o confirmada
                            if (reserva.Estado == EstadoReserva.Cancelada || reserva.Estado == EstadoReserva.Completada)
                            {
                                return BadRequest(new { message = "No se puede cancelar una reserva ya completada o cancelada" });
                            }
                            
                            // Actualizar disponibilidad si se cancela
                            var disponibilidadReserva = await _unitOfWork.DisponibilidadHoraria.GetByIdAsync(reserva.DisponibilidadId);
                            if (disponibilidadReserva != null)
                            {
                                disponibilidadReserva.CambiarEstado(EstadoDisponibilidad.Disponible);
                            }
                        }
                        else if (nuevoEstado == EstadoReserva.Confirmada)
                        {
                            // Solo el paseador puede confirmar
                            if (reserva.PaseadorId != userId && !esAdmin)
                            {
                                return Forbid();
                            }
                            
                            if (reserva.Estado != EstadoReserva.Pendiente)
                            {
                                return BadRequest(new { message = "Solo se pueden confirmar reservas pendientes" });
                            }
                        }
                        else if (nuevoEstado == EstadoReserva.Completada)
                        {
                            // Solo el paseador puede marcar como completada
                            if (reserva.PaseadorId != userId && !esAdmin)
                            {
                                return Forbid();
                            }
                            
                            if (reserva.Estado != EstadoReserva.Confirmada)
                            {
                                return BadRequest(new { message = "Solo se pueden completar reservas confirmadas" });
                            }
                        }
                        
                        // Cambiar estado
                        reserva.CambiarEstado(nuevoEstado);
                    }
                    else
                    {
                        return BadRequest(new { message = "Estado no válido" });
                    }
                }
                
                // Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                
                // Obtener datos para respuesta
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(reserva.UsuarioId);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId);
                var perro = await _unitOfWork.Perros.GetByIdAsync(reserva.PerroId);
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(reserva.ServicioId);
                
                // Usar el método auxiliar en lugar de declarar una variable
                var fechaServicio = await ObtenerFechaServicioAsync(reserva.DisponibilidadId);
                
                var reservaDto = new ReservaDto
                {
                    Id = reserva.Id,
                    UsuarioId = reserva.UsuarioId,
                    NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                    PaseadorId = reserva.PaseadorId,
                    NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                    PerroId = reserva.PerroId,
                    NombrePerro = perro.Nombre,
                    ServicioId = reserva.ServicioId,
                    NombreServicio = servicio.Nombre,
                    FechaReserva = reserva.FechaReserva,
                    FechaServicio = fechaServicio,
                    Estado = reserva.Estado.ToString(),
                    Precio = reserva.Precio.Cantidad,
                    DireccionRecogida = dto.DireccionRecogida ?? "Dirección de recogida", // Esto debería venir de otro lado
                    DireccionEntrega = dto.DireccionEntrega ?? "Dirección de entrega"    // Esto debería venir de otro lado
                };
                
                return Ok(reservaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar reserva: {ex.Message}" });
            }
        }

        // Método auxiliar para mapear reservas a DTOs
        private async Task<List<ReservaDto>> MapReservasADtos(IEnumerable<Reserva> reservas)
        {
            var result = new List<ReservaDto>();
            
            foreach (var reserva in reservas)
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(reserva.UsuarioId);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(reserva.PaseadorId);
                var perro = await _unitOfWork.Perros.GetByIdAsync(reserva.PerroId);
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(reserva.ServicioId);
                
                // En lugar de declarar una variable, usar el método auxiliar
                var fechaServicio = await ObtenerFechaServicioAsync(reserva.DisponibilidadId);
                
                result.Add(new ReservaDto
                {
                    Id = reserva.Id,
                    UsuarioId = reserva.UsuarioId,
                    NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                    PaseadorId = reserva.PaseadorId,
                    NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                    PerroId = reserva.PerroId,
                    NombrePerro = perro.Nombre,
                    ServicioId = reserva.ServicioId,
                    NombreServicio = servicio.Nombre,
                    FechaReserva = reserva.FechaReserva,
                    FechaServicio = fechaServicio, // Usar la fecha obtenida del método auxiliar
                    Estado = reserva.Estado.ToString(),
                    Precio = reserva.Precio.Cantidad,
                    DireccionRecogida = "Dirección de recogida",
                    DireccionEntrega = "Dirección de entrega"
                });
            }
            
            return result;
        }
        
        // Endpoint para obtener disponibilidad de un paseador
        [HttpGet("disponibilidad/{paseadorId}")]
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
                    disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorYFechaAsync(paseadorId, fechaInicio);
                }
                else
                {
                    // Si no hay fecha, obtener toda la disponibilidad futura
                    disponibilidades = await _unitOfWork.DisponibilidadHoraria.GetByPaseadorIdAsync(paseadorId);
                    disponibilidades = disponibilidades.Where(d => d.FechaHora >= DateTime.Now && d.Estado == EstadoDisponibilidad.Disponible);
                }

                // Convertir a formato más amigable
                var result = disponibilidades
                    .OrderBy(d => d.FechaHora)
                    .Select(d => new
                    {
                        Id = d.Id,
                        FechaHora = d.FechaHora,
                        Fecha = d.FechaHora.ToString("yyyy-MM-dd"),
                        Hora = d.FechaHora.ToString("HH:mm"),
                        Estado = d.Estado.ToString()
                    })
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener disponibilidad: {ex.Message}" });
            }
        }
        
        // Obtener historial de reservas completadas
        [HttpGet("historial")]
        [Authorize]
        public async Task<IActionResult> GetHistorialReservas()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                IEnumerable<Reserva> reservas;
                
                if (User.IsInRole("Paseador"))
                {
                    // Si es paseador, obtener sus reservas como paseador
                    reservas = await _unitOfWork.Reservas.GetByPaseadorIdAsync(userId);
                }
                else
                {
                    // Si es usuario normal, obtener sus reservas como cliente
                    reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(userId);
                }
                
                // Filtrar solo las completadas
                reservas = reservas.Where(r => r.Estado == EstadoReserva.Completada);
                
                var reservasDto = await MapReservasADtos(reservas);
                
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener historial de reservas: {ex.Message}" });
            }
        }
        
        // Obtener reservas pendientes y confirmadas (activas)
        [HttpGet("activas")]
        [Authorize]
        public async Task<IActionResult> GetReservasActivas()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                IEnumerable<Reserva> reservas;
                
                if (User.IsInRole("Paseador"))
                {
                    // Si es paseador, obtener sus reservas como paseador
                    reservas = await _unitOfWork.Reservas.GetByPaseadorIdAsync(userId);
                }
                else
                {
                    // Si es usuario normal, obtener sus reservas como cliente
                    reservas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(userId);
                }
                
                // Filtrar solo las activas (pendientes y confirmadas)
                reservas = reservas.Where(r => 
                    r.Estado == EstadoReserva.Pendiente || 
                    r.Estado == EstadoReserva.Confirmada);
                
                var reservasDto = await MapReservasADtos(reservas);
                
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener reservas activas: {ex.Message}" });
            }
        }
        
        // Cancelar una reserva
        [HttpPost("{id}/cancelar")]
        [Authorize]
        public async Task<IActionResult> CancelarReserva(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                
                if (reserva == null)
                {
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                // Verificar permisos
                bool esPropietario = reserva.UsuarioId == userId || reserva.PaseadorId == userId;
                bool esAdmin = User.IsInRole("Administrador");
                
                if (!esPropietario && !esAdmin)
                {
                    return Forbid();
                }
                
                // Verificar que se pueda cancelar
                if (reserva.Estado == EstadoReserva.Cancelada || reserva.Estado == EstadoReserva.Completada)
                {
                    return BadRequest(new { message = "No se puede cancelar una reserva ya completada o cancelada" });
                }
                
                // Actualizar disponibilidad
                var disponibilidadReserva = await _unitOfWork.DisponibilidadHoraria.GetByIdAsync(reserva.DisponibilidadId);
                if (disponibilidadReserva != null)
                {
                    disponibilidadReserva.CambiarEstado(EstadoDisponibilidad.Disponible);
                }
                
                // Cambiar estado de la reserva
                reserva.CambiarEstado(EstadoReserva.Cancelada);
                
                // Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { message = "Reserva cancelada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al cancelar reserva: {ex.Message}" });
            }
        }
        
        // Confirmar una reserva (solo paseador)
        [HttpPost("{id}/confirmar")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> ConfirmarReserva(Guid id)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                
                if (reserva == null)
                {
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                // Verificar que la reserva pertenece al paseador
                if (reserva.PaseadorId != paseadorId && !User.IsInRole("Administrador"))
                {
                    return Forbid();
                }
                
                // Verificar que se pueda confirmar
                if (reserva.Estado != EstadoReserva.Pendiente)
                {
                    return BadRequest(new { message = "Solo se pueden confirmar reservas pendientes" });
                }
                
                // Cambiar estado de la reserva
                reserva.CambiarEstado(EstadoReserva.Confirmada);
                
                // Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { message = "Reserva confirmada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al confirmar reserva: {ex.Message}" });
            }
        }
        
        // Completar una reserva (solo paseador)
        [HttpPost("{id}/completar")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> CompletarReserva(Guid id)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(id);
                
                if (reserva == null)
                {
                    return NotFound(new { message = "Reserva no encontrada" });
                }
                
                // Verificar que la reserva pertenece al paseador
                if (reserva.PaseadorId != paseadorId && !User.IsInRole("Administrador"))
                {
                    return Forbid();
                }
                
                // Verificar que se pueda completar
                if (reserva.Estado != EstadoReserva.Confirmada)
                {
                    return BadRequest(new { message = "Solo se pueden completar reservas confirmadas" });
                }
                
                // Cambiar estado de la reserva
                reserva.CambiarEstado(EstadoReserva.Completada);
                
                // Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { message = "Reserva completada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al completar reserva: {ex.Message}" });
            }
        }

        // Método auxiliar para obtener la fecha de servicio desde DisponibilidadHoraria
        private async Task<DateTime> ObtenerFechaServicioAsync(Guid disponibilidadId)
        {
            var disp = await _unitOfWork.DisponibilidadHoraria.GetByIdAsync(disponibilidadId);
            return disp?.FechaHora ?? DateTime.MinValue; // Retorna la fecha o un valor por defecto si es null
        }
    }
}
