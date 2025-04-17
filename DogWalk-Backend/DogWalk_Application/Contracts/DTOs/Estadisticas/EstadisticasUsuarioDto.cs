using System;

namespace DogWalk_Application.Contracts.DTOs.Estadisticas
{
    public class EstadisticasUsuarioDto
    {
        public int TotalReservas { get; set; }
        public decimal TotalGastado { get; set; }
        public int NumeroPerros { get; set; }
        public int ReservasPendientes { get; set; }
        public int ReservasCompletadas { get; set; }
        public List<KeyValuePair<string, int>> ServiciosMasUsados { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, int>> PaseadoresFavoritos { get; set; } = new List<KeyValuePair<string, int>>();
    }
}
