using System;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IPaseadorRepository : IRepository<Paseador>
    {
        Task<Paseador> GetByEmailAsync(string email);
        Task<IEnumerable<Paseador>> GetByValoracionMinimaAsync(decimal valoracionMinima);
        Task<IEnumerable<Paseador>> GetByDistanciaAsync(double latitud, double longitud, double distanciaMaximaKm);
        Task<bool> ExisteEmailAsync(string email);
        Task<bool> ExisteDniAsync(string dni);
        Task<IEnumerable<Paseador>> GetWithDisponibilidadAsync(DateTime fecha);
        Task<IEnumerable<Paseador>> GetWithServicioAsync(Guid servicioId);
        Task<IEnumerable<Paseador>> GetByDisponibilidadAsync(bool disponible = true);
        Task<IEnumerable<Paseador>> GetByFechaAsync(DateTime fecha);
        Task<(IEnumerable<Paseador> Paseadores, int Total)> GetPaginadosAsync(int numeroPagina, int elementosPorPagina);
    }
