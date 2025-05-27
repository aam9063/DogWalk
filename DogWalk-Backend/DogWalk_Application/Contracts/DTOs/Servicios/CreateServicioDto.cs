namespace DogWalk_Application.Contracts.DTOs.Servicios
{
    /// <summary>
    /// DTO para crear un nuevo servicio.
    /// </summary>
    public class CreateServicioDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; } // "Paseo", "GuarderiaDia", "GuarderiaNoche", etc.
        public decimal PrecioReferencia { get; set; }
    }
}