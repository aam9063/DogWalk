using System;
using System.Threading;
using System.Threading.Tasks;

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
    IRefreshTokenRepository RefreshTokens { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<T> AddAsync<T>(T entity) where T : class;
}