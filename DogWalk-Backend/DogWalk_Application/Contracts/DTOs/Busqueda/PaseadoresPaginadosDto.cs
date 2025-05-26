using DogWalk_Application.Contracts.DTOs.Busqueda;

namespace DogWalk_Application.Contracts.DTOs.Paseadores
{
    public class PaseadoresPaginadosDto : ResultadoPaginadoDto<PaseadorDto>
    {
        public int TotalPaseadoresActivos { get; set; }
        public decimal ValoracionPromedio { get; set; }
    }
}