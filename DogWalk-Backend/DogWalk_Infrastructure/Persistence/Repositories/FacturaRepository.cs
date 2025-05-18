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

public class FacturaRepository : RepositoryBase<Factura>, IFacturaRepository
{
    public FacturaRepository(DogWalkDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Factura>> GetAllAsync()
    {
        return await _context.Facturas
            .Include(f => f.Detalles)
            .ToListAsync();
    }

    public async Task<IEnumerable<Factura>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Facturas
            .Where(f => f.UsuarioId == usuarioId)
            .Include(f => f.Detalles)
            .ToListAsync();
    }

    public async Task<IEnumerable<Factura>> GetByFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Facturas
            .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin)
            .OrderByDescending(f => f.FechaFactura)
            .ToListAsync();
    }

    public override async Task<Factura> GetByIdAsync(Guid id)
    {
        return await _context.Facturas
            .Include(f => f.Usuario)
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<decimal> GetTotalVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
    {
        var facturas = await _context.Facturas
            .Where(f => f.FechaFactura >= fechaInicio && f.FechaFactura <= fechaFin)
            .ToListAsync();
            
        return facturas.Sum(f => f.Total.Cantidad);
    }

    public async Task<int> GetCantidadFacturasPorMetodoPago(MetodoPago metodoPago)
    {
        return await _context.Facturas
            .CountAsync(f => f.MetodoPago == metodoPago);
    }
}
