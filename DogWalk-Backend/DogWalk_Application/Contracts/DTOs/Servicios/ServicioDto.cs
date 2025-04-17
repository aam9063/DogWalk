namespace DogWalk_Application.Contracts.DTOs.Servicios
{
    public class ServicioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; } // "Paseo", "GuarderiaDia", "GuarderiaNoche", etc.
        public decimal PrecioReferencia { get; set; } // Precio de referencia o mínimo
    }
}