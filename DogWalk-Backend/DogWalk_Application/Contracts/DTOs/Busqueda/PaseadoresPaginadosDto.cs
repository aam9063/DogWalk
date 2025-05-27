using DogWalk_Application.Contracts.DTOs.Busqueda;

namespace DogWalk_Application.Contracts.DTOs.Paseadores
{
    /// <summary>
    /// DTO para representar los paseadores paginados.
    /// </summary>
    public class PaseadoresPaginadosDto : ResultadoPaginadoDto<PaseadorDto>
    {
        public int TotalPaseadoresActivos { get; set; }
        public decimal ValoracionPromedio { get; set; }
    }
}