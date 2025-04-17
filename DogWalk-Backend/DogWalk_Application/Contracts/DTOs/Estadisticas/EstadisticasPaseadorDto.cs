using System;

namespace DogWalk_Application.Contracts.DTOs.Estadisticas
{
    public class EstadisticasPaseadorDto
    {
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal ValoracionPromedio { get; set; }
        public int TotalValoraciones { get; set; }
        public int ReservasPendientes { get; set; }
        public int ReservasCompletadas { get; set; }
        public List<KeyValuePair<string, int>> ServiciosMasReservados { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<DateTime, int>> ReservasPorDia { get; set; } = new List<KeyValuePair<DateTime, int>>();
    }
}
