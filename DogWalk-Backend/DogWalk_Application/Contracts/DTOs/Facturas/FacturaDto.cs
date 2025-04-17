namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    public class FacturaDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public string Estado { get; set; } // "Pagada", "Pendiente", "Cancelada"
        public List<DetalleFacturaDto> Detalles { get; set; } = new List<DetalleFacturaDto>();
    }
}
