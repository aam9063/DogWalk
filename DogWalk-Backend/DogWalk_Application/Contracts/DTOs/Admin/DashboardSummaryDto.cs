namespace DogWalk_Application.Contracts.DTOs.Admin
{
    public class DashboardSummaryDto
    {
        public int TotalUsuarios { get; set; }
        public int TotalPaseadores { get; set; }
        public int TotalReservas { get; set; }
        public int ReservasCompletadas { get; set; }
        public int ReservasPendientes { get; set; }
        public int ReservasCanceladas { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal IngresosMensuales { get; set; }
        public int TotalArticulosVendidos { get; set; }
        public List<KeyValuePair<string, int>> ServiciosMasPopulares { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, int>> PaseadoresMasReservados { get; set; } = new List<KeyValuePair<string, int>>();
    }
}