using DogWalk_Application.Contracts.DTOs.Chat;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace DogWalk_API.Hubs
{
    /// <summary>
    /// Hub de chat para la comunicación entre usuarios y paseadores.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del hub de chat.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja el evento de conexión del usuario.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Envia un mensaje de chat.
        /// </summary>
        /// <param name="mensaje">Datos del mensaje a enviar.</param>
        [Authorize]
        public async Task EnviarMensaje(EnviarMensajeDto mensaje)
        {
            try
            {
                if (mensaje == null || string.IsNullOrEmpty(mensaje.Mensaje) || mensaje.EnviadorId == Guid.Empty)
                    throw new HubException("Datos de mensaje inválidos");

                var emisorId = mensaje.EnviadorId;
                var receptorId = mensaje.DestinatarioId;
                var tipoEmisor = mensaje.TipoEmisor; // "Usuario" o "Paseador"

                // Verificar si el destinatario existe según el tipo
                bool destinatarioExiste = false;
                if (mensaje.TipoDestinatario == "Paseador")
                    destinatarioExiste = await _unitOfWork.Paseadores.GetByIdAsync(receptorId) != null;
                else if (mensaje.TipoDestinatario == "Usuario")
                    destinatarioExiste = await _unitOfWork.Usuarios.GetByIdAsync(receptorId) != null;

                if (!destinatarioExiste)
                    throw new HubException($"No se encontró el destinatario con ID {receptorId}");

                // Asignar correctamente usuario y paseador
                Guid usuarioId, paseadorId;
                if (tipoEmisor == "Usuario")
                {
                    usuarioId = emisorId;
                    paseadorId = receptorId;
                }
                else
                {
                    usuarioId = receptorId;
                    paseadorId = emisorId;
                }

                var chatMensaje = new ChatMensaje(
                    Guid.NewGuid(),
                    usuarioId,
                    paseadorId,
                    mensaje.Mensaje
                );

                await _unitOfWork.ChatMensajes.AddAsync(chatMensaje);
                await _unitOfWork.SaveChangesAsync();

                // Obtener datos del emisor para enviar en la respuesta
                string nombreEmisor = "Usuario";
                string fotoEmisor = "";

                if (tipoEmisor == "Usuario")
                {
                    var usuario = await _unitOfWork.Usuarios.GetByIdAsync(emisorId);
                    if (usuario != null)
                    {
                        nombreEmisor = $"{usuario.Nombre} {usuario.Apellido}";
                        fotoEmisor = usuario.FotoPerfil ?? "";
                    }
                }
                else
                {
                    var paseador = await _unitOfWork.Paseadores.GetByIdAsync(emisorId);
                    if (paseador != null)
                    {
                        nombreEmisor = $"{paseador.Nombre} {paseador.Apellido}";
                        fotoEmisor = paseador.FotoPerfil ?? "";
                    }
                }

                var mensajeDto = new ChatMensajeDto
                {
                    Id = chatMensaje.Id,
                    EnviadorId = emisorId,
                    NombreEnviador = nombreEmisor,
                    FotoEnviador = fotoEmisor,
                    TipoEnviador = tipoEmisor,
                    Mensaje = chatMensaje.Mensaje,
                    FechaHora = chatMensaje.FechaHora,
                    Leido = false
                };

                // Enviar mensaje al emisor y al receptor
                await Clients.Group(emisorId.ToString()).SendAsync("RecibirMensaje", mensajeDto);
                await Clients.Group(receptorId.ToString()).SendAsync("RecibirMensaje", mensajeDto);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error en EnviarMensaje: {ex}");
                throw new HubException($"Error al enviar mensaje: {ex.Message}");
            }
        }

        /// <summary>
        /// Marca un mensaje como leído.
        /// </summary>
        /// <param name="mensajeId">ID del mensaje a marcar como leído.</param>
        [Authorize]
        public async Task MarcarLeido(Guid mensajeId)
        {
            var usuarioId = Guid.Parse(Context.User.FindFirst("sub").Value);
            var rol = Context.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            var mensaje = await _unitOfWork.ChatMensajes.GetByIdAsync(mensajeId);
            if (mensaje == null)
            {
                throw new HubException("Mensaje no encontrado");
            }

            if (rol == "Usuario" && mensaje.UsuarioId == usuarioId)
            {
                mensaje.MarcarComoLeidoUsuario();
            }
            else if (rol == "Paseador" && mensaje.PaseadorId == usuarioId)
            {
                mensaje.MarcarComoLeidoPaseador();
            }
            else
            {
                throw new HubException("No tienes permiso para marcar este mensaje como leído");
            }

            await _unitOfWork.SaveChangesAsync();

            // Notificar que el mensaje ha sido leído
            await Clients.Group(mensaje.UsuarioId.ToString()).SendAsync("MensajeLeido", mensajeId);
            await Clients.Group(mensaje.PaseadorId.ToString()).SendAsync("MensajeLeido", mensajeId);
        }
    }
}
