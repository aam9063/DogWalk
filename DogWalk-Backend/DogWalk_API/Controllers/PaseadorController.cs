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
using DogWalk_Infrastructure.Authentication;
using DogWalk_Application.Features.Paseadores.Queries;
using MediatR;
using DogWalk_Application.Contracts.DTOs.Busqueda;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaseadorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtProvider _jwtProvider;
        private readonly IMediator _mediator;

        public PaseadorController(IUnitOfWork unitOfWork, JwtProvider jwtProvider, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
            _mediator = mediator;
        }

        // Registro de paseadores (público)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterPaseadorDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar contraseña
                if (dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest(new { message = "Las contraseñas no coinciden" });
                }

                // Verificar si el email ya está registrado
                var usuarioExistente = await _unitOfWork.Usuarios.GetByEmailAsync(dto.Email);
                if (usuarioExistente != null)
                {
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                // Crear objetos de valor
                var dni = Dni.Create(dto.Dni);
                var direccion = Direccion.Create(dto.Direccion);
                var email = Email.Create(dto.Email);
                var telefono = Telefono.Create(dto.Telefono);
                var password = Password.Create(dto.Password);
                var coordenadas = Coordenadas.Create(dto.Latitud, dto.Longitud);

                // Crear usuario nuevo (ajustando orden de parámetros)
                var nuevoUsuario = new Usuario(
                    Guid.NewGuid(),
                    dni,
                    dto.Nombre,
                    dto.Apellido,
                    direccion,
                    email,
                    password,
                    telefono,
                    RolUsuario.Paseador  // Rol debe ser el último parámetro
                );

                // Crear paseador con todos los parámetros requeridos
                var nuevoPaseador = new Paseador(
                    nuevoUsuario.Id,
                    dni,
                    dto.Nombre,
                    dto.Apellido,
                    direccion,
                    email,
                    password,
                    coordenadas,
                    telefono
                );

                // Asignar servicios y precios
                if (dto.Servicios != null && dto.Servicios.Any())
                {
                    foreach (var servicio in dto.Servicios)
                    {
                        // Verificar que el servicio existe
                        var servicioExistente = await _unitOfWork.Servicios.GetByIdAsync(servicio.ServicioId);
                        if (servicioExistente != null)
                        {
                            // Crear precio para este servicio
                            var nuevoPrecio = new Precio(
                                Guid.NewGuid(),
                                nuevoPaseador.Id,
                                servicio.ServicioId,
                                Dinero.Create(servicio.Precio)
                            );

                            // Agregar precio a paseador
                            // Verifica si este método necesita ser implementado en tu entidad Paseador
                            nuevoPaseador.AgregarPrecio(nuevoPrecio);
                        }
                    }
                }

                // Persistir usuario y paseador
                await _unitOfWork.Usuarios.AddAsync(nuevoUsuario);
                await _unitOfWork.Paseadores.AddAsync(nuevoPaseador);
                await _unitOfWork.SaveChangesAsync();

                // Generar JWT token con tu JwtProvider existente
                var token = _jwtProvider.GenerateToken(nuevoUsuario);

                return Ok(new
                {
                    Token = token,
                    Usuario = new
                    {
                        Id = nuevoUsuario.Id,
                        Nombre = nuevoUsuario.Nombre,
                        Apellido = nuevoUsuario.Apellido,
                        Email = nuevoUsuario.Email.ToString(),
                        Rol = nuevoUsuario.Rol.ToString()
                    }
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
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(userId);
                
                if (paseador == null)
                {
                    return NotFound(new { message = "Perfil de paseador no encontrado" });
                }
                
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
                
                // Obtener valoración promedio (llamar el método directamente)
                double valoracionPromedio = await _unitOfWork.RankingPaseadores.GetPromedioPaseadorAsync(userId);
                
                // Obtener las valoraciones y contarlas después
                var valoraciones = await _unitOfWork.RankingPaseadores.GetByPaseadorIdAsync(userId);
                int cantidadValoraciones = valoraciones.Count();
                
                var result = new
                {
                    Id = paseador.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email.ToString(),
                    Telefono = usuario.Telefono.ToString(),
                    Direccion = usuario.Direccion.ToString(),
                    Coordenadas = new
                    {
                        Latitud = paseador.Ubicacion.Latitud,
                        Longitud = paseador.Ubicacion.Longitud
                    },
                    // Asignar un valor fijo o usar una propiedad adecuada de tu entidad
                    RadioServicio = 5.0,
                    // Usar el valor ya calculado
                    ValoracionPromedio = valoracionPromedio,
                    // Usar el valor ya calculado
                    CantidadValoraciones = cantidadValoraciones,
                    Servicios = paseador.Precios.Select(p => new
                    {
                        Id = p.ServicioId,
                        Nombre = p.Servicio.Nombre,
                        Descripcion = p.Servicio.Descripcion,
                        Tipo = p.Servicio.Tipo.ToString(),
                        Precio = p.Valor.Cantidad
                    }).ToList()
                };
                
                return Ok(result);
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
        public async Task<IActionResult> UpdatePrecios([FromBody] List<DogWalk_Application.Contracts.DTOs.Paseadores.ActualizarPrecioDto> precios)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(userId);
                
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }
                
                foreach (var precioDto in precios)
                {
                    // Verificar que el servicio existe
                    var servicio = await _unitOfWork.Servicios.GetByIdAsync(precioDto.ServicioId);
                    if (servicio == null)
                    {
                        return BadRequest(new { message = $"Servicio con ID {precioDto.ServicioId} no encontrado" });
                    }
                    
                    // Buscar si ya existe un precio para este servicio
                    var precioExistente = paseador.Precios.FirstOrDefault(p => p.ServicioId == precioDto.ServicioId);
                    
                    if (precioExistente != null)
                    {
                        // Actualizar precio existente
                        precioExistente.ActualizarPrecio(Dinero.Create(precioDto.Precio));
                    }
                    else
                    {
                        // Crear nuevo precio
                        var nuevoPrecio = new Precio(
                            Guid.NewGuid(),
                            userId,
                            precioDto.ServicioId,
                            Dinero.Create(precioDto.Precio)
                        );
                        
                        // Asegúrate de que este método exista o implementa la lógica adecuada aquí
                        paseador.AgregarPrecio(nuevoPrecio);
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

        // Crear nuevo precio para un servicio
        [HttpPost("precios")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> CreatePrecio([FromBody] DogWalk_Application.Contracts.DTOs.Paseadores.ActualizarPrecioDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                // Verificar que el paseador existe
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(userId);
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
                
                // Verificar si ya existe un precio para este servicio
                if (paseador.Precios.Any(p => p.ServicioId == dto.ServicioId))
                {
                    return BadRequest(new { message = "Ya existe un precio para este servicio. Use PUT para actualizar." });
                }
                
                // Crear nuevo precio
                var nuevoPrecio = new Precio(
                    Guid.NewGuid(),
                    userId,
                    dto.ServicioId,
                    Dinero.Create(dto.Precio)
                );
                
                // Agregar el precio al paseador
                paseador.AgregarPrecio(nuevoPrecio);
                
                // Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                
                return Ok(new { 
                    Id = nuevoPrecio.Id,
                    PaseadorId = nuevoPrecio.PaseadorId,
                    ServicioId = nuevoPrecio.ServicioId,
                    Precio = nuevoPrecio.Valor.Cantidad
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear precio: {ex.Message}" });
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

        // DogWalk_API/Controllers/PaseadorController.cs - Agregar este endpoint
        [HttpGet("buscar")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultadoPaginadoDto<PaseadorMapDto>>> BuscarPaseadores(
            [FromQuery] string codigoPostal = null,
            [FromQuery] DateTime? fechaEntrega = null,
            [FromQuery] DateTime? fechaRecogida = null,
            [FromQuery] int? cantidadPerros = null,
            [FromQuery] Guid? servicioId = null,
            [FromQuery] double? latitud = null,
            [FromQuery] double? longitud = null,
            [FromQuery] double? distanciaMaxima = 10.0,
            [FromQuery] decimal? valoracionMinima = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10)
        {
            try
            {
                var query = new BuscarPaseadoresQuery
                {
                    CodigoPostal = codigoPostal,
                    FechaEntrega = fechaEntrega,
                    FechaRecogida = fechaRecogida,
                    CantidadPerros = cantidadPerros,
                    ServicioId = servicioId,
                    Latitud = latitud,
                    Longitud = longitud,
                    DistanciaMaxima = distanciaMaxima,
                    ValoracionMinima = valoracionMinima,
                    Pagina = pagina,
                    ElementosPorPagina = elementosPorPagina
                };
                
                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al buscar paseadores: {ex.Message}" });
            }
        }
    }
}
