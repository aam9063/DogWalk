using DogWalk_Application.Contracts.DTOs.Chat;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador para la interacción con el chat.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del controlador de chat.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para la base de datos</param>
        public ChatController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todas las conversaciones del usuario.
        /// </summary>
        /// <returns>Lista de conversaciones</returns>
        [HttpGet("conversaciones")]
        public async Task<ActionResult<List<ResumenConversacionDto>>> GetConversaciones()
        {
            try
            {
                Console.WriteLine("==== CLAIMS RECIBIDOS ====");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
                Console.WriteLine("==========================");

                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (subClaim == null || string.IsNullOrWhiteSpace(subClaim.Value))
                {
                    Console.WriteLine("No se encontró el claim 'nameidentifier' o está vacío");
                    return Unauthorized("No se encontró el claim 'nameidentifier' o está vacío");
                }
                Guid usuarioId;
                try
                {
                    usuarioId = Guid.Parse(subClaim.Value);
                }
                catch
                {
                    Console.WriteLine("El claim 'nameidentifier' no es un GUID válido: " + subClaim.Value);
                    return Unauthorized("El claim 'nameidentifier' no es un GUID válido: " + subClaim.Value);
                }

                var rolClaim = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (rolClaim == null || string.IsNullOrWhiteSpace(rolClaim.Value))
                {
                    Console.WriteLine("No se encontró el claim 'role' o está vacío");
                    return Unauthorized("No se encontró el claim 'role' o está vacío");
                }
                var rol = rolClaim.Value;

                if (rol == "Usuario")
                {
                    var mensajes = await _unitOfWork.ChatMensajes.GetMensajesUsuarioAsync(usuarioId);
                    if (mensajes == null)
                    {
                        Console.WriteLine("mensajes es null");
                        return Ok(new List<ResumenConversacionDto>());
                    }
                    var conversaciones = new Dictionary<Guid, ResumenConversacionDto>();

                    foreach (var paseadorId in mensajes.Select(m => m.PaseadorId).Distinct())
                    {
                        var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                        if (paseador == null)
                        {
                            Console.WriteLine($"Paseador con ID {paseadorId} no encontrado");
                            continue;
                        }

                        var mensajesPaseador = mensajes.Where(m => m.PaseadorId == paseadorId).OrderByDescending(m => m.FechaHora).ToList();
                        var ultimoMensaje = mensajesPaseador.FirstOrDefault();

                        conversaciones[paseadorId] = new ResumenConversacionDto
                        {
                            UsuarioId = usuarioId,
                            PaseadorId = paseadorId,
                            NombreContacto = $"{paseador.Nombre} {paseador.Apellido}",
                            FotoContacto = paseador.FotoPerfil,
                            UltimoMensaje = ultimoMensaje?.Mensaje,
                            FechaUltimoMensaje = ultimoMensaje?.FechaHora ?? DateTime.UtcNow,
                            MensajesNoLeidos = mensajesPaseador.Count(m => !m.LeidoPorUsuario)
                        };
                    }

                    return Ok(conversaciones.Values.OrderByDescending(c => c.FechaUltimoMensaje).ToList());
                }
                else if (rol == "Paseador")
                {
                    var mensajes = await _unitOfWork.ChatMensajes.GetMensajesPaseadorAsync(usuarioId);
                    if (mensajes == null)
                    {
                        Console.WriteLine("mensajes es null");
                        return Ok(new List<ResumenConversacionDto>());
                    }
                    var conversaciones = new Dictionary<Guid, ResumenConversacionDto>();

                    foreach (var clienteId in mensajes.Select(m => m.UsuarioId).Distinct())
                    {
                        var cliente = await _unitOfWork.Usuarios.GetByIdAsync(clienteId);
                        if (cliente == null)
                        {
                            Console.WriteLine($"Usuario con ID {clienteId} no encontrado");
                            continue;
                        }

                        var mensajesCliente = mensajes.Where(m => m.UsuarioId == clienteId).OrderByDescending(m => m.FechaHora).ToList();
                        var ultimoMensaje = mensajesCliente.FirstOrDefault();

                        conversaciones[clienteId] = new ResumenConversacionDto
                        {
                            UsuarioId = clienteId,
                            PaseadorId = usuarioId,
                            NombreContacto = $"{cliente.Nombre} {cliente.Apellido}",
                            FotoContacto = cliente.FotoPerfil,
                            UltimoMensaje = ultimoMensaje?.Mensaje,
                            FechaUltimoMensaje = ultimoMensaje?.FechaHora ?? DateTime.UtcNow,
                            MensajesNoLeidos = mensajesCliente.Count(m => !m.LeidoPorPaseador)
                        };
                    }

                    return Ok(conversaciones.Values.OrderByDescending(c => c.FechaUltimoMensaje).ToList());
                }

                Console.WriteLine("Rol no permitido: " + rol);
                return Forbid();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en GetConversaciones: " + ex);
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        /// <summary>
        /// Obtiene una conversación específica.
        /// </summary>
        /// <param name="contactoId">ID del contacto</param>
        /// <returns>Conversación</returns>
        [HttpGet("conversacion/{contactoId}")]
        public async Task<ActionResult<ConversacionDto>> GetConversacion(Guid contactoId)
        {
            try
            {
                Console.WriteLine("==== CLAIMS RECIBIDOS ====");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
                Console.WriteLine("==========================");

                var subClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (subClaim == null || string.IsNullOrWhiteSpace(subClaim.Value))
                {
                    Console.WriteLine("No se encontró el claim 'nameidentifier' o está vacío");
                    return Unauthorized("No se encontró el claim 'nameidentifier' o está vacío");
                }
                Guid usuarioId;
                try
                {
                    usuarioId = Guid.Parse(subClaim.Value);
                }
                catch
                {
                    Console.WriteLine("El claim 'nameidentifier' no es un GUID válido: " + subClaim.Value);
                    return Unauthorized("El claim 'nameidentifier' no es un GUID válido: " + subClaim.Value);
                }

                var rolClaim = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (rolClaim == null || string.IsNullOrWhiteSpace(rolClaim.Value))
                {
                    Console.WriteLine("No se encontró el claim 'role' o está vacío");
                    return Unauthorized("No se encontró el claim 'role' o está vacío");
                }
                var rol = rolClaim.Value;

                if (rol == "Usuario")
                {
                    var paseador = await _unitOfWork.Paseadores.GetByIdAsync(contactoId);
                    if (paseador == null)
                    {
                        Console.WriteLine($"Paseador con ID {contactoId} no encontrado");
                        return NotFound("Paseador no encontrado");
                    }

                    var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
                    if (usuario == null)
                    {
                        Console.WriteLine($"Usuario con ID {usuarioId} no encontrado");
                        return NotFound("Usuario no encontrado");
                    }

                    var mensajes = await _unitOfWork.ChatMensajes.GetConversacionAsync(usuarioId, contactoId);
                    if (mensajes == null)
                    {
                        Console.WriteLine("mensajes es null");
                        mensajes = new List<ChatMensaje>();
                    }

                    // Marcar mensajes como leídos
                    await _unitOfWork.ChatMensajes.MarcarLeidosPorUsuarioAsync(usuarioId, contactoId);
                    await _unitOfWork.SaveChangesAsync();

                    var conversacionDto = new ConversacionDto
                    {
                        UsuarioId = usuarioId,
                        NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                        FotoUsuario = usuario.FotoPerfil,
                        PaseadorId = contactoId,
                        NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                        FotoPaseador = paseador.FotoPerfil,
                        Mensajes = mensajes.Select(m => new ChatMensajeDto
                        {
                            Id = m.Id,
                            EnviadorId = m.UsuarioId == usuarioId ? m.UsuarioId : m.PaseadorId,
                            NombreEnviador = m.UsuarioId == usuarioId ? $"{usuario.Nombre} {usuario.Apellido}" : $"{paseador.Nombre} {paseador.Apellido}",
                            FotoEnviador = m.UsuarioId == usuarioId ? usuario.FotoPerfil : paseador.FotoPerfil,
                            TipoEnviador = m.UsuarioId == usuarioId ? "Usuario" : "Paseador",
                            Mensaje = m.Mensaje,
                            FechaHora = m.FechaHora,
                            Leido = m.UsuarioId == usuarioId ? m.LeidoPorPaseador : m.LeidoPorUsuario
                        }).ToList()
                    };

                    return Ok(conversacionDto);
                }
                else if (rol == "Paseador")
                {
                    var usuario = await _unitOfWork.Usuarios.GetByIdAsync(contactoId);
                    if (usuario == null)
                    {
                        Console.WriteLine($"Usuario con ID {contactoId} no encontrado");
                        return NotFound("Usuario no encontrado");
                    }

                    var paseador = await _unitOfWork.Paseadores.GetByIdAsync(usuarioId);
                    if (paseador == null)
                    {
                        Console.WriteLine($"Paseador con ID {usuarioId} no encontrado");
                        return NotFound("Paseador no encontrado");
                    }

                    var mensajes = await _unitOfWork.ChatMensajes.GetConversacionAsync(contactoId, usuarioId);
                    if (mensajes == null)
                    {
                        Console.WriteLine("mensajes es null");
                        mensajes = new List<ChatMensaje>();
                    }

                    // Marcar mensajes como leídos
                    await _unitOfWork.ChatMensajes.MarcarLeidosPorPaseadorAsync(contactoId, usuarioId);
                    await _unitOfWork.SaveChangesAsync();

                    var conversacionDto = new ConversacionDto
                    {
                        UsuarioId = contactoId,
                        NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}",
                        FotoUsuario = usuario.FotoPerfil,
                        PaseadorId = usuarioId,
                        NombrePaseador = $"{paseador.Nombre} {paseador.Apellido}",
                        FotoPaseador = paseador.FotoPerfil,
                        Mensajes = mensajes.Select(m => new ChatMensajeDto
                        {
                            Id = m.Id,
                            EnviadorId = m.PaseadorId == usuarioId ? m.PaseadorId : m.UsuarioId,
                            NombreEnviador = m.PaseadorId == usuarioId ? $"{paseador.Nombre} {paseador.Apellido}" : $"{usuario.Nombre} {usuario.Apellido}",
                            FotoEnviador = m.PaseadorId == usuarioId ? paseador.FotoPerfil : usuario.FotoPerfil,
                            TipoEnviador = m.PaseadorId == usuarioId ? "Paseador" : "Usuario",
                            Mensaje = m.Mensaje,
                            FechaHora = m.FechaHora,
                            Leido = m.PaseadorId == usuarioId ? m.LeidoPorUsuario : m.LeidoPorPaseador
                        }).ToList()
                    };

                    return Ok(conversacionDto);
                }

                Console.WriteLine("Rol no permitido: " + rol);
                return Forbid();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en GetConversacion: " + ex);
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }
    }
}
