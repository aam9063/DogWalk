namespace DogWalk_Application.Contracts.DTOs.Servicios
{
    /// <summary>
    /// DTO para representar un servicio.
    /// </summary>
    public class ServicioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; } // "Paseo", "GuarderiaDia", "GuarderiaNoche", etc.
        public decimal PrecioReferencia { get; set; } // Precio de referencia o mínimo
    }
}