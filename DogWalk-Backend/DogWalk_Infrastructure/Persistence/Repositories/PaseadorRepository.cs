using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;

namespace DogWalk_Infrastructure.Persistence.Repositories
{
    public class PaseadorRepository : GenericRepository<Paseador>, IPaseadorRepository
    {
        public PaseadorRepository(DogWalkDbContext context) : base(context)
        {
        }
        
        public async Task<Paseador> GetByEmailAsync(string email)
        {
            return await _context.Paseadores
                .FirstOrDefaultAsync(p => p.Email.Valor == email);
        }
        
        public async Task<IEnumerable<Paseador>> GetByValoracionMinimaAsync(decimal valoracionMinima)
        {
            return await _context.Paseadores
                .Where(p => p.ValoracionGeneral >= valoracionMinima)
                .OrderByDescending(p => p.ValoracionGeneral)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Paseador>> GetByDistanciaAsync(double latitud, double longitud, double distanciaMaximaKm)
        {
            // Simplificado para SQL Server - podría optimizarse con funciones geoespaciales
            // Fórmula de distancia haversine simplificada
            // Esta consulta no es óptima para distancias largas o cerca de los polos pero funciona
            // para distancias moderadas en áreas urbanas
            
            var paseadores = await _context.Paseadores.ToListAsync();
            
            // Filtrado en memoria - en un escenario real usaríamos una función SQL optimizada
            return paseadores.Where(p => 
            {
                double latP = p.Ubicacion.Latitud;
                double lonP = p.Ubicacion.Longitud;
                
                // Cálculo aproximado de distancia en km (fórmula haversine simplificada)
                double radiusTierra = 6371; // km
                double dLat = ToRadians(latitud - latP);
                double dLon = ToRadians(longitud - lonP);
                
                double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                           Math.Cos(ToRadians(latP)) * Math.Cos(ToRadians(latitud)) *
                           Math.Sin(dLon/2) * Math.Sin(dLon/2);
                
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
                double distancia = radiusTierra * c;
                
                return distancia <= distanciaMaximaKm;
            });
        }
        
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        
        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _context.Paseadores
                .AnyAsync(p => p.Email.Valor == email);
        }
        
        public async Task<bool> ExisteDniAsync(string dni)
        {
            return await _context.Paseadores
                .AnyAsync(p => p.Dni.Valor == dni);
        }
        
        public override async Task<Paseador> GetByIdAsync(Guid id)
        {
            return await _context.Paseadores
                .Include(p => p.Precios)
                    .ThenInclude(p => p.Servicio)
                .Include(p => p.Disponibilidad)
                .Include(p => p.ValoracionesRecibidas)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<IEnumerable<Paseador>> GetWithDisponibilidadAsync(DateTime fecha)
        {
            return await _context.Paseadores
                .Include(p => p.Disponibilidad)
                .Where(p => p.Disponibilidad.Any(d => 
                    d.FechaHora.Date == fecha.Date && 
                    d.Estado == EstadoDisponibilidad.Disponible))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Paseador>> GetWithServicioAsync(Guid servicioId)
        {
            return await _context.Paseadores
                .Include(p => p.Precios)
                .Where(p => p.Precios.Any(pr => pr.ServicioId == servicioId))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Paseador>> GetByDisponibilidadAsync(bool disponible = true)
        {
            if (disponible)
            {
                return await _context.Paseadores
                    .Include(p => p.Disponibilidad)
                    .Where(p => p.Disponibilidad.Any(d => d.Estado == EstadoDisponibilidad.Disponible))
                    .ToListAsync();
            }
            else
            {
                return await _context.Paseadores
                    .Include(p => p.Disponibilidad)
                    .Where(p => !p.Disponibilidad.Any(d => d.Estado == EstadoDisponibilidad.Disponible))
                    .ToListAsync();
            }
        }
        
        public async Task<IEnumerable<Paseador>> GetByFechaAsync(DateTime fecha)
        {
            return await _context.Paseadores
                .Include(p => p.Disponibilidad)
                .Where(p => p.Disponibilidad.Any(d => d.FechaHora.Date == fecha.Date))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Paseador> Paseadores, int Total)> GetPaginadosAsync(int numeroPagina, int elementosPorPagina)
    {
        var query = _context.Paseadores.AsQueryable();
        
        // Obtener el total antes de paginar
        var total = await query.CountAsync();
        
        // Aplicar paginación
        var paseadores = await query
            .OrderBy(p => p.Apellido) // Puedes cambiar el ordenamiento según necesites
            .Skip((numeroPagina - 1) * elementosPorPagina)
            .Take(elementosPorPagina)
            .ToListAsync();

        return (paseadores, total);
    }
    }
}
