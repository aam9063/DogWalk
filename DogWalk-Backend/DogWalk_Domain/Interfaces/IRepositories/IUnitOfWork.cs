using System;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Domain.Interfaces.IRepositories;

/// <summary>
/// Interfaz para el unit of work.
/// </summary>
public interface IUnitOfWork : IDisposable
{   
    /// <summary>
    /// Repositorio de usuarios.
    /// </summary>
    IUsuarioRepository Usuarios { get; }

    /// <summary>
    /// Repositorio de paseadores.
    /// </summary>
    IPaseadorRepository Paseadores { get; }

    /// <summary>
    /// Repositorio de perros.
    /// </summary>
    IPerroRepository Perros { get; }

    /// <summary>
    /// Repositorio de servicios.
    /// </summary>
    IServicioRepository Servicios { get; }

    /// <summary>
    /// Repositorio de artículos.
    /// </summary>
    IArticuloRepository Articulos { get; }

    /// <summary>
    /// Repositorio de reservas.
    /// </summary>
    IReservaRepository Reservas { get; }

    /// <summary>
    /// Repositorio de facturas.
    /// </summary>
    IFacturaRepository Facturas { get; }

    /// <summary>
    /// Repositorio de mensajes de chat.
    /// </summary>
    IChatMensajeRepository ChatMensajes { get; }

    /// <summary>
    /// Repositorio de disponibilidad horaria.
    /// </summary>
    IDisponibilidadHorariaRepository DisponibilidadHoraria { get; }

    /// <summary>
    /// Repositorio de opiniones de perros.
    /// </summary>
    IOpinionPerroRepository OpinionesPerros { get; }

    /// <summary>
    /// Repositorio de ranking de paseadores.
    /// </summary>
    IRankingPaseadorRepository RankingPaseadores { get; }

    /// <summary>
    /// Repositorio de refresh tokens.
    /// </summary>
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// Guarda los cambios en la base de datos.
    /// </summary>  
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Confirma una transacción.
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Revierte una transacción.
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Agrega una entidad a la base de datos.
    /// </summary>
    Task<T> AddAsync<T>(T entity) where T : class;
}