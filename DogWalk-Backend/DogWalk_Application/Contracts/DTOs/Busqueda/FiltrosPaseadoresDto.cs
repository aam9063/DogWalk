using System;

namespace DogWalk_Application.Contracts.DTOs.Busqueda
{
    /// <summary>
    /// DTO para representar los filtros de búsqueda de paseadores.
    /// </summary>
    public class FiltrosPaseadoresDto
    {
        public string Busqueda { get; set; } // Texto libre de búsqueda
        public decimal? ValoracionMinima { get; set; } // Filtrar por valoración mínima
        public double? Latitud { get; set; } // Ubicación del usuario
        public double? Longitud { get; set; }
        public double? DistanciaMaxima { get; set; } // Distancia máxima en km
        public Guid? ServicioId { get; set; } // Filtrar por servicio específico
        public DateTime? FechaDisponibilidad { get; set; } // Filtrar por disponibilidad en fecha
        public decimal? PrecioMaximo { get; set; } // Filtrar por precio máximo
        public int Pagina { get; set; } = 1;
        public int ElementosPorPagina { get; set; } = 10;
    }
}
