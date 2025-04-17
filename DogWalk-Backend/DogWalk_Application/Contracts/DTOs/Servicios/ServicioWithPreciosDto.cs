using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Servicios
{
    public class ServicioWithPreciosDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public List<PrecioPaseadorDto> Precios { get; set; } = new List<PrecioPaseadorDto>();
        public decimal PrecioMinimo { get; set; }
        public decimal PrecioMaximo { get; set; }
        public decimal PrecioPromedio { get; set; }
    }

    public class PrecioPaseadorDto
    {
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public string FotoPaseador { get; set; }
        public decimal Precio { get; set; }
        public decimal ValoracionPaseador { get; set; }
    }
}