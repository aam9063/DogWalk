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
using DogWalk_Application.Contracts.DTOs.Estadisticas;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con los paseadores de perros.
    /// Incluye registro, gestión de perfil, servicios, precios y disponibilidad.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PaseadorController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtProvider _jwtProvider;
        private readonly IMediator _mediator;

        /// <summary>
        /// Constructor del controlador de paseadores.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        /// <param name="jwtProvider">Proveedor de servicios JWT</param>
        /// <param name="mediator">Mediador para el patrón CQRS</param>
        public PaseadorController(IUnitOfWork unitOfWork, JwtProvider jwtProvider, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
            _mediator = mediator;
        }

        /// <summary>
        /// Registra un nuevo paseador en el sistema.
        /// </summary>
        /// <param name="dto">Datos de registro del paseador</param>
        /// <returns>Token de autenticación y datos básicos del paseador registrado</returns>
        /// <response code="200">Retorna el token y datos del paseador si el registro es exitoso</response>
        /// <response code="400">Si los datos son inválidos o el email ya está registrado</response>
        /// <response code="500">Si ocurre un error interno en el servidor</response>
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
                    RolUsuario.Paseador  
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

        /// <summary>
        /// Obtiene el perfil completo del paseador autenticado.
        /// </summary>
        /// <returns>Información detallada del perfil del paseador, incluyendo valoraciones y servicios</returns>
        /// <response code="200">Retorna el perfil completo del paseador</response>
        /// <response code="404">Si el perfil del paseador no se encuentra</response>
        /// <response code="401">Si el usuario no está autenticado</response>
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

        /// <summary>
        /// Obtiene el perfil público de un paseador específico.
        /// </summary>
        /// <param name="id">ID del paseador</param>
        /// <returns>Información pública del perfil del paseador</returns>
        /// <response code="200">Retorna el perfil público del paseador</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        [HttpGet("public/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaseadorPublicProfile(Guid id)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(id);

                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }

                // Obtener valoración promedio
                double valoracionPromedio = await _unitOfWork.RankingPaseadores.GetPromedioPaseadorAsync(id);

                // Obtener las valoraciones
                var valoraciones = await _unitOfWork.RankingPaseadores.GetByPaseadorIdAsync(id);
                int cantidadValoraciones = valoraciones.Count();

                var result = new
                {
                    Id = paseador.Id,
                    Nombre = paseador.Nombre,
                    Apellido = paseador.Apellido,
                    FotoPerfil = paseador.FotoPerfil,
                    Coordenadas = new
                    {
                        Latitud = paseador.Ubicacion.Latitud,
                        Longitud = paseador.Ubicacion.Longitud
                    },
                    RadioServicio = 5.0, // O el valor que corresponda
                    ValoracionPromedio = valoracionPromedio,
                    CantidadValoraciones = cantidadValoraciones,
                    Servicios = paseador.Precios.Select(p => new
                    {
                        Id = p.ServicioId,
                        Nombre = p.Servicio.Nombre,
                        Descripcion = p.Servicio.Descripcion,
                        Tipo = p.Servicio.Tipo.ToString(),
                        Precio = p.Valor.Cantidad
                    }).ToList(),
                    // Incluir las últimas valoraciones
                    UltimasValoraciones = valoraciones.OrderByDescending(v => v.CreadoEn)
                        .Take(5) // Limitamos a las últimas 5 valoraciones
                        .Select(v => new
                        {
                            Puntuacion = v.Valoracion.Puntuacion,
                            Comentario = v.Comentario,
                            Fecha = v.CreadoEn,
                            Usuario = new
                            {
                                Nombre = v.Usuario.Nombre,
                                Apellido = v.Usuario.Apellido,
                                FotoPerfil = v.Usuario.FotoPerfil
                            }
                        }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener perfil del paseador: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene detalles específicos de un paseador, incluyendo servicios y disponibilidad.
        /// </summary>
        /// <param name="id">ID del paseador</param>
        /// <returns>Detalles completos del paseador</returns>
        /// <response code="200">Retorna los detalles del paseador</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        [HttpGet("details/{id}")]
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
                        Comentario = "",
                        Fecha = v.CreadoEn
                    }).ToList(),

                    ReservasProximas = new List<ReservaDto>()
                };

                return Ok(paseadorDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener detalles del paseador: {ex.Message}" });
            }
        }

        /// <summary>
        /// Actualiza el perfil del paseador autenticado.
        /// </summary>
        /// <param name="updateDto">Datos actualizados del paseador</param>
        /// <returns>Confirmación de la actualización</returns>
        /// <response code="200">Si el perfil se actualizó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="404">Si el paseador no se encuentra</response>
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

                // Actualizar información personal
                var direccion = !string.IsNullOrEmpty(updateDto.Direccion)
                    ? Direccion.Create(updateDto.Direccion)
                    : null;

                var telefono = !string.IsNullOrEmpty(updateDto.Telefono)
                    ? Telefono.Create(updateDto.Telefono)
                    : null;

                paseador.ActualizarInformacionPersonal(
                    updateDto.Nombre,
                    updateDto.Apellido,
                    direccion,
                    telefono
                );

                // Actualizar ubicación si se proporcionaron coordenadas
                if (updateDto.Latitud != 0 && updateDto.Longitud != 0)
                {
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

        /// <summary>
        /// Actualiza los precios de los servicios del paseador.
        /// </summary>
        /// <param name="precios">Lista de precios actualizados</param>
        /// <returns>Confirmación de la actualización de precios</returns>
        /// <response code="200">Si los precios se actualizaron correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
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

        /// <summary>
        /// Crea un nuevo precio para un servicio del paseador.
        /// </summary>
        /// <param name="dto">Datos del nuevo precio</param>
        /// <returns>Confirmación de la creación del precio</returns>
        /// <response code="200">Si el precio se creó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
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

                return Ok(new
                {
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

        /// <summary>
        /// Crea una nueva disponibilidad horaria para el paseador.
        /// </summary>
        /// <param name="disponibilidadDto">Datos de la nueva disponibilidad</param>
        /// <returns>Confirmación de la creación de la disponibilidad</returns>
        /// <response code="200">Si la disponibilidad se creó correctamente</response>
        /// <response code="400">Si los datos son inválidos</response>
        /// <response code="401">Si el usuario no está autenticado</response>
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

                return Ok(new
                {
                    message = "Disponibilidad creada correctamente",
                    disponibilidades = disponibilidadesCreadas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al crear disponibilidad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de todos los paseadores.
        /// </summary>
        /// <param name="paginacionParams">Parámetros de paginación</param>
        /// <returns>Lista paginada de paseadores</returns>
        /// <response code="200">Retorna la lista de paseadores</response>
        /// <response code="400">Si los parámetros de paginación son inválidos</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaseadores([FromQuery] PaginacionParams paginacionParams)
        {
            try
            {
                var (paseadores, total) = await _unitOfWork.Paseadores.GetPaginadosAsync(
                    paginacionParams.PageNumber,
                    paginacionParams.PageSize
                );

                var paseadoresDto = paseadores.Select(p => new PaseadorDto
                {
                    Id = p.Id,
                    Dni = p.Dni.ToString(),
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    Direccion = p.Direccion.ToString(),
                    Email = p.Email.ToString(),
                    Telefono = p.Telefono?.ToString(),
                    FotoPerfil = p.FotoPerfil,
                    ValoracionGeneral = p.ValoracionGeneral,
                    Latitud = p.Ubicacion.Latitud,
                    Longitud = p.Ubicacion.Longitud,
                    FechaRegistro = p.CreadoEn
                }).ToList();

                var resultado = new PaseadoresPaginadosDto
                {
                    Items = paseadoresDto,
                    TotalItems = total,
                    PaginaActual = paginacionParams.PageNumber,
                    ElementosPorPagina = paginacionParams.PageSize,
                    TotalPaginas = (int)Math.Ceiling(total / (double)paginacionParams.PageSize),
                    // Opcional: calcular estadísticas adicionales
                    TotalPaseadoresActivos = total,
                    ValoracionPromedio = paseadoresDto.Any() ? paseadoresDto.Average(p => p.ValoracionGeneral) : 0
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener paseadores: {ex.Message}" });
            }
        }

        /// <summary>
        /// Obtiene las reservas asociadas al paseador autenticado.
        /// </summary>
        /// <returns>Lista de reservas del paseador</returns>
        /// <response code="200">Retorna la lista de reservas</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        [HttpGet("reservas")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> GetReservas()
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                var reservas = await _unitOfWork.Reservas.GetByPaseadorIdAsync(paseadorId);

                var reservasDto = new List<ReservaDto>();

                foreach (var reserva in reservas)
                {
                    reservasDto.Add(new ReservaDto
                    {
                        Id = reserva.Id,
                        UsuarioId = reserva.UsuarioId,
                        NombreUsuario = $"{reserva.Usuario?.Nombre} {reserva.Usuario?.Apellido}".Trim(),
                        PaseadorId = reserva.PaseadorId,
                        NombrePaseador = $"{reserva.Paseador?.Nombre} {reserva.Paseador?.Apellido}".Trim(),
                        PerroId = reserva.PerroId,
                        NombrePerro = reserva.Perro?.Nombre,
                        ServicioId = reserva.ServicioId,
                        NombreServicio = reserva.Servicio?.Nombre,
                        FechaReserva = reserva.FechaReserva,
                        FechaServicio = reserva.Disponibilidad?.FechaHora ?? DateTime.MinValue,
                        Estado = reserva.Estado.ToString(),
                        Precio = reserva.Precio.Cantidad,
                        DireccionRecogida = reserva.Usuario?.Direccion?.TextoCompleto ?? "No especificada",
                        DireccionEntrega = reserva.Usuario?.Direccion?.TextoCompleto ?? "No especificada"
                    });
                }

                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener reservas: {ex.Message}" });
            }
        }

        /// <summary>
        /// Busca paseadores según criterios específicos.
        /// </summary>
        /// <param name="codigoPostal">Código postal para la búsqueda</param>
        /// <param name="fechaEntrega">Fecha de entrega del perro</param>
        /// <param name="fechaRecogida">Fecha de recogida del perro</param>
        /// <param name="cantidadPerros">Cantidad de perros para el servicio</param>
        /// <param name="servicioId">ID del servicio requerido</param>
        /// <param name="latitud">Latitud para búsqueda por ubicación</param>
        /// <param name="longitud">Longitud para búsqueda por ubicación</param>
        /// <param name="distanciaMaxima">Distancia máxima en kilómetros (default: 10)</param>
        /// <param name="valoracionMinima">Valoración mínima requerida</param>
        /// <param name="pagina">Número de página (default: 1)</param>
        /// <param name="elementosPorPagina">Elementos por página (default: 10)</param>
        /// <returns>Lista paginada de paseadores que cumplen con los criterios</returns>
        /// <response code="200">Retorna la lista de paseadores filtrada</response>
        /// <response code="400">Si los parámetros de búsqueda son inválidos</response>
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

        /// <summary>
        /// Obtiene estadísticas del dashboard para el paseador autenticado.
        /// </summary>
        /// <returns>Estadísticas y métricas del paseador</returns>
        /// <response code="200">Retorna las estadísticas del dashboard</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Paseador")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var paseadorId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

                // Obtener todas las reservas del paseador
                var reservas = await _unitOfWork.Reservas.GetByPaseadorIdAsync(paseadorId);

                // Obtener valoraciones
                var valoraciones = await _unitOfWork.RankingPaseadores.GetByPaseadorIdAsync(paseadorId);

                var estadisticas = new EstadisticasPaseadorDto
                {
                    TotalReservas = reservas.Count(),
                    TotalIngresos = reservas.Where(r => r.Estado == EstadoReserva.Completada)
                                          .Sum(r => r.Precio.Cantidad),
                    ValoracionPromedio = (decimal)await _unitOfWork.RankingPaseadores.GetPromedioPaseadorAsync(paseadorId),
                    TotalValoraciones = valoraciones.Count(),
                    ReservasPendientes = reservas.Count(r => r.Estado == EstadoReserva.Pendiente),
                    ReservasCompletadas = reservas.Count(r => r.Estado == EstadoReserva.Completada),

                    // Agrupar servicios más reservados
                    ServiciosMasReservados = reservas.GroupBy(r => r.Servicio.Nombre)
                                                   .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                                                   .OrderByDescending(x => x.Value)
                                                   .ToList(),

                    // Agrupar reservas por día
                    ReservasPorDia = reservas.GroupBy(r => r.FechaReserva.Date)
                                            .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                                            .OrderBy(x => x.Key)
                                            .ToList()
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener dashboard: {ex.Message}" });
            }
        }
    }
}
