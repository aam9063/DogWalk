using DogWalk_Domain.Common.Enums;

namespace DogWalk_Application.Contracts.DTOs.Carrito;

public class CheckoutDto
{
    public MetodoPago MetodoPago { get; set; }
    public DireccionEnvioDto DireccionEnvio { get; set; }
}

public class DireccionEnvioDto
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Calle { get; set; }
    public string Ciudad { get; set; }
    public string CodigoPostal { get; set; }
    public string Provincia { get; set; }
    public string Telefono { get; set; }
    public bool GuardarDireccion { get; set; }
}