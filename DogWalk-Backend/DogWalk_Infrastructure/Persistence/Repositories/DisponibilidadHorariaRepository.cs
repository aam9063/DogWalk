using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class DisponibilidadHorariaRepository : GenericRepository<DisponibilidadHoraria>, IDisponibilidadHorariaRepository
{
    public DisponibilidadHorariaRepository(DogWalkDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DisponibilidadHoraria>> GetByPaseadorIdAsync(Guid paseadorId)
    {
        return await _context.DisponibilidadHoraria
            .Where(d => d.PaseadorId == paseadorId)
            .OrderBy(d => d.FechaHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<DisponibilidadHoraria>> GetByEstadoAsync(EstadoDisponibilidad estado)
    {
        return await _context.DisponibilidadHoraria
            .Where(d => d.Estado == estado)
            .OrderBy(d => d.FechaHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<DisponibilidadHoraria>> GetByFechaAsync(DateTime fecha)
    {
        return await _context.DisponibilidadHoraria
            .Where(d => d.FechaHora.Date == fecha.Date)
            .OrderBy(d => d.FechaHora)
            .ToListAsync();
    }

    public async Task<IEnumerable<DisponibilidadHoraria>> GetByPaseadorYFechaAsync(Guid paseadorId, DateTime fecha)
    {
        return await _context.DisponibilidadHoraria
            .Where(d => d.PaseadorId == paseadorId && d.FechaHora.Date == fecha.Date)
            .OrderBy(d => d.FechaHora)
            .ToListAsync();
    }

    public async Task<bool> ExisteDisponibilidad(Guid paseadorId, DateTime fechaHora)
    {
        return await _context.DisponibilidadHoraria
            .AnyAsync(d => d.PaseadorId == paseadorId && 
                           d.FechaHora == fechaHora && 
                           d.Estado == EstadoDisponibilidad.Disponible);
    }
}
