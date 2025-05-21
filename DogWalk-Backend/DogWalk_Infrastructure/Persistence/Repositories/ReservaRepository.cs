using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories
{
    public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
    {
        public ReservaRepository(DogWalkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reserva>> GetByUsuarioIdAsync(Guid usuarioId)
        {
            return await _context.Reservas
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetByPaseadorIdAsync(Guid paseadorId)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Disponibilidad)
                .Where(r => r.PaseadorId == paseadorId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetByEstadoAsync(EstadoReserva estado)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Where(r => r.Estado == estado)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetByFechaAsync(DateTime fecha)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Where(r => r.FechaReserva.Date == fecha.Date)
                .OrderBy(r => r.FechaReserva)
                .ToListAsync();
        }

        public override async Task<Reserva> GetByIdAsync(Guid id)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Include(r => r.Perro)
                .Include(r => r.Disponibilidad)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reserva>> GetByPerioDoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Where(r => r.FechaReserva >= fechaInicio && r.FechaReserva <= fechaFin)
                .OrderBy(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetByPerroIdAsync(Guid perroId)
        {
            return await _context.Reservas
                .Include(r => r.Paseador)
                .Include(r => r.Servicio)
                .Where(r => r.PerroId == perroId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }
    }
}
