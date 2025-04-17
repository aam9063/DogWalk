using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories
{
    public interface IDisponibilidadHorariaRepository : IRepository<DisponibilidadHoraria>
    {
        Task<IEnumerable<DisponibilidadHoraria>> GetByPaseadorIdAsync(Guid paseadorId);
        Task<IEnumerable<DisponibilidadHoraria>> GetByEstadoAsync(EstadoDisponibilidad estado);
        Task<IEnumerable<DisponibilidadHoraria>> GetByFechaAsync(DateTime fecha);
        Task<IEnumerable<DisponibilidadHoraria>> GetByPaseadorYFechaAsync(Guid paseadorId, DateTime fecha);
        Task<bool> ExisteDisponibilidad(Guid paseadorId, DateTime fechaHora);
    }
}
