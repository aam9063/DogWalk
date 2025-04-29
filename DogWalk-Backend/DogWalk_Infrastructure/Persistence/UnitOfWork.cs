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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DogWalkDbContext _context;
        private IDbContextTransaction _transaction;
        
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
        
        public UnitOfWork(DogWalkDbContext context)
        {
            _context = context;
        }
        
        public IUsuarioRepository Usuarios => _usuarioRepository ??= new UsuarioRepository(_context);
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
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        
        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        
        public void Dispose()
        {
            _context.Dispose();
            _transaction?.Dispose();
            GC.SuppressFinalize(this);
        }

        public DbContext GetDbContext()
        {
            return _context;
        }
    }
}
