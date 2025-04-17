using System;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    IPaseadorRepository Paseadores { get; }
    IPerroRepository Perros { get; }
    IServicioRepository Servicios { get; }
    IArticuloRepository Articulos { get; }
    IReservaRepository Reservas { get; }
    IFacturaRepository Facturas { get; }
    IChatMensajeRepository ChatMensajes { get; }
    IDisponibilidadHorariaRepository DisponibilidadHoraria { get; }
    IOpinionPerroRepository OpinionesPerros { get; }
    IRankingPaseadorRepository RankingPaseadores { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}