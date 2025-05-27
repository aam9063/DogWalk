using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    /// <summary>
    /// DTO para representar un resumen de valoraciones.
    /// </summary>
    public class ResumenValoracionesDto
    {
        public decimal PromedioValoracion { get; set; }
        public int CantidadValoraciones { get; set; }
        public Dictionary<int, int> DistribucionValoraciones { get; set; } = new Dictionary<int, int>();
    }
}
