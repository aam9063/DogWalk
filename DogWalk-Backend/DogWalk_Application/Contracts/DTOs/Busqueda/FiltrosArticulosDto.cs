using System;

namespace DogWalk_Application.Contracts.DTOs.Busqueda
{
    public class FiltrosArticulosDto
    {
        public string Busqueda { get; set; }
        public string Categoria { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public bool SoloDisponibles { get; set; } = true;
        public int Pagina { get; set; } = 1;
        public int ElementosPorPagina { get; set; } = 12;
    }
}
