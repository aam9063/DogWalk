using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories
{
    public interface IChatMensajeRepository : IRepository<ChatMensaje>
    {
        Task<IEnumerable<ChatMensaje>> GetConversacionAsync(Guid usuarioId, Guid paseadorId);
        Task<IEnumerable<ChatMensaje>> GetMensajesUsuarioAsync(Guid usuarioId);
        Task<IEnumerable<ChatMensaje>> GetMensajesPaseadorAsync(Guid paseadorId);
        Task<IEnumerable<ChatMensaje>> GetMensajesNoLeidosUsuarioAsync(Guid usuarioId);
        Task<IEnumerable<ChatMensaje>> GetMensajesNoLeidosPaseadorAsync(Guid paseadorId);
        Task MarcarLeidosPorUsuarioAsync(Guid usuarioId, Guid paseadorId);
        Task MarcarLeidosPorPaseadorAsync(Guid usuarioId, Guid paseadorId);
    }
}
