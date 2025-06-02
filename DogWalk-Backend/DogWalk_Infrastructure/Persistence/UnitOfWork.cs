using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using DogWalk_Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace DogWalk_Infrastructure.Persistence
{
    /// <summary>
    /// Implementación del unit of work.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Contexto de la base de datos.
        /// </summary>
        private readonly DogWalkDbContext _context;

        /// <summary>
        /// Transacción de la base de datos.
        /// </summary>
        private IDbContextTransaction _transaction;

        /// <summary>
        /// Repositorio de usuarios.
        /// </summary>
        private IUsuarioRepository _usuarioRepository;
        private IPaseadorRepository _paseadorRepository;
        private IPerroRepository _perroRepository;
        private IServicioRepository _servicioRepository;
        private IArticuloRepository _articuloRepository;
        private IReservaRepository _reservaRepository;
        private IFacturaRepository _facturaRepository;
        private IChatMensajeRepository _chatMensajeRepository;
        private IDisponibilidadHorariaRepository _disponibilidadHorariaRepository;
        private IOpinionPerroRepository _opinionPerroRepository;
        private IRankingPaseadorRepository _rankingPaseadorRepository;
        private IRefreshTokenRepository _refreshTokenRepository;

        /// <summary>
        /// Constructor del unit of work.
        /// </summary>
        public UnitOfWork(
            DogWalkDbContext context,
            IUsuarioRepository usuarios,
            IArticuloRepository articulos,
            IRefreshTokenRepository refreshTokenRepository
        )
        {
            _context = context;
            _usuarioRepository = usuarios;
            _articuloRepository = articulos;
            _refreshTokenRepository = refreshTokenRepository;
        }

        /// <summary>
        /// Repositorio de usuarios.
        /// </summary>
        public IUsuarioRepository Usuarios => _usuarioRepository ??= new UsuarioRepository(_context);

        /// <summary>
        /// Repositorio de paseadores.
        /// </summary>
        public IPaseadorRepository Paseadores => _paseadorRepository ??= new PaseadorRepository(_context);
        public IPerroRepository Perros => _perroRepository ??= new PerroRepository(_context);
        public IServicioRepository Servicios => _servicioRepository ??= new ServicioRepository(_context);
        public IArticuloRepository Articulos => _articuloRepository ??= new ArticuloRepository(_context);
        public IReservaRepository Reservas => _reservaRepository ??= new ReservaRepository(_context);
        public IFacturaRepository Facturas => _facturaRepository ??= new FacturaRepository(_context);
        public IChatMensajeRepository ChatMensajes => _chatMensajeRepository ??= new ChatMensajeRepository(_context);
        public IDisponibilidadHorariaRepository DisponibilidadHoraria => _disponibilidadHorariaRepository ??= new DisponibilidadHorariaRepository(_context);
        public IOpinionPerroRepository OpinionesPerros => _opinionPerroRepository ??= new OpinionPerroRepository(_context);
        public IRankingPaseadorRepository RankingPaseadores => _rankingPaseadorRepository ??= new RankingPaseadorRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);

        /// <summary>
        /// Contexto de la base de datos.
        /// </summary>
        public DogWalkDbContext Context => _context;

        /// <summary>
        /// Inicia una transacción.
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Confirma una transacción.
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction?.CommitAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// Revierte una transacción.
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction?.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        /// <summary>
        /// Guarda los cambios en la base de datos.
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega una entidad a la base de datos.
        /// </summary>
        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        /// <summary>
        /// Elimina una entidad de la base de datos.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
            _transaction?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Obtiene el contexto de la base de datos.
        /// </summary>
        public DbContext GetDbContext()
        {
            return _context;
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }
    }
}
