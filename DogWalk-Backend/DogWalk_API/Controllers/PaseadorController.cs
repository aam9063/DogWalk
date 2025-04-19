using DogWalk_Application.Contracts.DTOs.Auth;
using DogWalk_Application.Contracts.DTOs.Disponibilidad;
using DogWalk_Application.Contracts.DTOs.Paseadores;
using DogWalk_Application.Contracts.DTOs.Precios;
using DogWalk_Application.Contracts.DTOs.Reservas;
using DogWalk_Application.Contracts.DTOs.Servicios;
using DogWalk_Application.Contracts.DTOs.Usuarios;
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
    public class PaseadorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaseadorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Registro de paseadores (público)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterPaseadorDto registerDto)
        {
            try
            {
                // Verificar si el email ya existe
                var existingPaseador = await _unitOfWork.Paseadores.GetByEmailAsync(registerDto.Email);
                if (existingPaseador != null)
                {
                    return BadRequest(new { message = "El email ya está registrado como paseador" });
                }

                // Verificar también en la tabla de usuarios
                var existingUser = await _unitOfWork.Usuarios.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "El email ya está registrado como usuario" });
                }

                // Verificar que las contraseñas coincidan
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Las contraseñas no coinciden" });
                }

                // Crear coordenadas
                // Como el constructor parece estar protegido, usamos el método estático Create si existe
                var coordenadas = Coordenadas.Create(registerDto.Latitud, registerDto.Longitud);

                // Crear paseador
                var paseador = new Paseador(
                    Guid.NewGuid(),
                    Dni.Create(registerDto.Dni),
                    registerDto.Nombre,
                    registerDto.Apellido,
                    Direccion.Create(registerDto.Direccion),
                    Email.Create(registerDto.Email),
                    Password.Create(registerDto.Password),
                    coordenadas,
                    Telefono.Create(registerDto.Telefono)
                );

                // Crear precios para los servicios si se proporcionaron
                if (registerDto.Servicios != null && registerDto.Servicios.Any())
                {
                    foreach (var servicioPrecio in registerDto.Servicios)
                    {
                        var servicio = await _unitOfWork.Servicios.GetByIdAsync(servicioPrecio.ServicioId);
                        if (servicio != null)
                        {
                            var precio = new Precio(
                                Guid.NewGuid(),
                                paseador.Id,
                                servicioPrecio.ServicioId,
                                Dinero.Create(servicioPrecio.Precio)
                            );
                            paseador.AgregarPrecio(precio);
                        }
                    }
                }
                else
                {
                    // Si no se proporcionaron servicios, agregamos precios por defecto para todos los servicios
                    var servicios = await _unitOfWork.Servicios.GetAllAsync();
                    foreach (var servicio in servicios)
                    {
                        var precio = new Precio(
                            Guid.NewGuid(),
                            paseador.Id,
                            servicio.Id,
                            Dinero.Create(10.0m)
                        );
                        paseador.AgregarPrecio(precio);
                    }
                }

                await _unitOfWork.Paseadores.AddAsync(paseador);
                await _unitOfWork.SaveChangesAsync();

                return Ok(new { 
                    message = "Paseador registrado correctamente", 
                    paseadorId = paseador.Id 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al registrar paseador: {ex.Message}" });
            }
        }

        // Obtener perfil de paseador (protegido)
        [HttpGet("profile")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Obtener ID del paseador desde el token
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                // Obtener valoraciones
                var valoraciones = await _unitOfWork.RankingPaseadores.GetByPaseadorIdAsync(paseadorId);
                
                // Crear DTO de perfil usando tu PaseadorProfileDto existente
                var profileDto = new PaseadorProfileDto
                {
                    Id = paseador.Id,
                    Nombre = paseador.Nombre,
                    Apellido = paseador.Apellido,
                    FotoPerfil = paseador.FotoPerfil,
                    ValoracionGeneral = paseador.ValoracionGeneral,
                    CantidadValoraciones = valoraciones.Count(),
                    Latitud = paseador.Ubicacion.Latitud,
                    Longitud = paseador.Ubicacion.Longitud,
                    Servicios = paseador.Precios.Select(p => new ServicioPrecioSimpleDto
                    {
                        ServicioId = p.ServicioId,
                        NombreServicio = p.Servicio.Nombre,
                        Precio = p.Valor.Cantidad
                    }).ToList()
                };

                return Ok(profileDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener perfil: {ex.Message}" });
            }
        }

        // Actualizar perfil de paseador (protegido)
        [HttpPut("profile")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdatePaseadorDto updateDto)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                // Actualizar ubicación si se proporcionaron coordenadas
                if (updateDto.Latitud != 0 && updateDto.Longitud != 0)
                {
                    // Usar el método estático Create para las coordenadas
                    var nuevasCoordenadas = Coordenadas.Create(updateDto.Latitud, updateDto.Longitud);
                    paseador.ActualizarUbicacion(nuevasCoordenadas);
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Perfil de paseador actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar perfil: {ex.Message}" });
            }
        }

        // Actualizar precios (protegido)
        [HttpPut("precios")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> UpdatePrecios([FromBody] List<ActualizarPrecioDto> preciosDto)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Verificar que todos los precios correspondan al paseador autenticado
                if (preciosDto.Any(p => p.PaseadorId != paseadorId))
                {
                    return BadRequest(new { message = "Solo puede actualizar sus propios precios" });
                }
                
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                // Actualizar precios
                foreach (var precioDto in preciosDto)
                {
                    var servicio = await _unitOfWork.Servicios.GetByIdAsync(precioDto.ServicioId);
                    if (servicio != null)
                    {
                        // Verificar si ya existe un precio para este servicio
                        var precioExistente = paseador.Precios.FirstOrDefault(p => p.ServicioId == precioDto.ServicioId);
                        
                        if (precioExistente != null)
                        {
                            // Actualizar precio existente
                            precioExistente.ActualizarPrecio(Dinero.Create(precioDto.Precio));
                        }
                        else
                        {
                            // Agregar nuevo precio
                            var nuevoPrecio = new Precio(
                                Guid.NewGuid(),
                                paseadorId,
                                precioDto.ServicioId,
                                Dinero.Create(precioDto.Precio)
                            );
                            paseador.AgregarPrecio(nuevoPrecio);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { message = "Precios actualizados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al actualizar precios: {ex.Message}" });
            }
        }

        // Criar disponibilidad (protegido)
        [HttpPost("disponibilidad")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> CreateDisponibilidad([FromBody] CrearDisponibilidadDto disponibilidadDto)
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                var disponibilidadesCreadas = new List<DisponibilidadHorariaDto>();
                
                // Crear slots de disponibilidad según el rango e intervalo
                DateTime currentTime = disponibilidadDto.FechaHoraInicio;
                while (currentTime < disponibilidadDto.FechaHoraFin)
                {
                    var disponibilidad = new DisponibilidadHoraria(
                        Guid.NewGuid(),
                        paseadorId,
                        currentTime,
                        EstadoDisponibilidad.Disponible
                    );
                    
                    // Agregar a la colección del paseador
                    paseador.AgregarDisponibilidad(disponibilidad);
                    
                    disponibilidadesCreadas.Add(new DisponibilidadHorariaDto
                    {
                        Id = disponibilidad.Id,
                        PaseadorId = paseadorId,
                        FechaHora = currentTime,
                        Estado = "Disponible"
                    });
                    
                    currentTime = currentTime.AddMinutes(disponibilidadDto.IntervaloMinutos);
                }

                await _unitOfWork.SaveChangesAsync();

                return Ok(new { 
                    message = "Disponibilidad creada correctamente", 
                    disponibilidades = disponibilidadesCreadas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear disponibilidad: {ex.Message}" });
            }
        }

        // Lista pública de paseadores (para usuarios)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaseadores()
        {
            try
            {
                // Obtener todos los paseadores
                var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
                var resultado = new List<PaseadorDto>();

                foreach (var paseador in paseadores)
                {
                    // Crear DTO para el listado usando tu PaseadorDto existente
                    resultado.Add(new PaseadorDto
                    {
                        Id = paseador.Id,
                        Dni = paseador.Dni.ToString(),
                        Nombre = paseador.Nombre,
                        Apellido = paseador.Apellido,
                        Direccion = paseador.Direccion.ToString(),
                        Email = paseador.Email.ToString(),
                        Telefono = paseador.Telefono?.ToString(),
                        FotoPerfil = paseador.FotoPerfil,
                        ValoracionGeneral = paseador.ValoracionGeneral,
                        Latitud = paseador.Ubicacion.Latitud,
                        Longitud = paseador.Ubicacion.Longitud,
                        FechaRegistro = paseador.CreadoEn
                    });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener paseadores: {ex.Message}" });
            }
        }

        // Detalles de un paseador específico (público)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaseadorDetails(Guid id)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(id);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                // Obtener valoraciones
                var valoraciones = await _unitOfWork.RankingPaseadores.GetByPaseadorIdAsync(id);
                
                // Reservas (nota: adaptado al modelo que te falta)
                // Esto implementa una versión simplificada
                
                // Crear DTO con los detalles usando tu PaseadorDetailsDto existente
                var paseadorDto = new PaseadorDetailsDto
                {
                    Id = paseador.Id,
                    Nombre = paseador.Nombre,
                    Apellido = paseador.Apellido,
                    Email = paseador.Email.ToString(),
                    Telefono = paseador.Telefono?.ToString(),
                    FotoPerfil = paseador.FotoPerfil,
                    ValoracionGeneral = paseador.ValoracionGeneral,
                    Latitud = paseador.Ubicacion.Latitud,
                    Longitud = paseador.Ubicacion.Longitud,
                    
                    // Mapeo de servicios y precios
                    Servicios = paseador.Precios.Select(p => new ServicioPrecioDto
                    {
                        ServicioId = p.ServicioId,
                        Precio = p.Valor.Cantidad
                    }).ToList(),
                    
                    // Mapeo de valoraciones (adaptado a tu modelo)
                    Valoraciones = valoraciones.Select(v => new ValoracionDto
                    {
                        Id = v.Id,
                        UsuarioId = v.UsuarioId,
                        NombreUsuario = $"{v.Usuario.Nombre} {v.Usuario.Apellido}",
                        FotoUsuario = v.Usuario.FotoPerfil,
                        Puntuacion = v.Valoracion.Puntuacion,
                        // Nota: Valoracion parece no tener un campo Comentario en tu modelo, 
                        // agregándolo vacío para que compile
                        Comentario = "", 
                        Fecha = v.CreadoEn
                    }).ToList(),
                    
                    // Implementación simplificada para ReservasProximas
                    ReservasProximas = new List<ReservaDto>()
                };

                return Ok(paseadorDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener detalles del paseador: {ex.Message}" });
            }
        }
        
        // Obtener reservas del paseador (protegido)
        [HttpGet("reservas")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> GetReservas()
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                var reservas = await _unitOfWork.Reservas.GetByPaseadorIdAsync(paseadorId);
                
                // Implementación simplificada para la lista de reservas
                var reservasDto = new List<ReservaDto>();
                
                foreach (var reserva in reservas)
                {
                    // Aquí debe ir el mapeo de reserva a ReservaDto según tu modelo
                    reservasDto.Add(new ReservaDto
                    {
                        Id = reserva.Id
                        // Completa con las propiedades específicas de tu ReservaDto
                    });
                }
                
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener reservas: {ex.Message}" });
            }
        }
    }
}
