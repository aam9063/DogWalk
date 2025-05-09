namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    public class FacturaDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string MetodoPago { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Total { get; set; }
    public List<DetalleFacturaDto> Detalles { get; set; }
}
}
