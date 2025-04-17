using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    public class ResumenValoracionesDto
    {
        public decimal PromedioValoracion { get; set; }
        public int CantidadValoraciones { get; set; }
        public Dictionary<int, int> DistribucionValoraciones { get; set; } = new Dictionary<int, int>();
        // DistribucionValoraciones: { 5: 10, 4: 5, 3: 3, 2: 1, 1: 0 } - Significa 10 valoraciones de 5 estrellas, 5 de 4 estrellas, etc.
    }
}
