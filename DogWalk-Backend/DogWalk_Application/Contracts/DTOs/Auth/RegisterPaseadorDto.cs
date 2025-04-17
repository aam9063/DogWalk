using System;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    public class RegisterPaseadorDto
    {
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Telefono { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        // Opcional: lista de servicios y precios iniciales
        public List<ServicioPrecioDto> Servicios { get; set; } = new List<ServicioPrecioDto>();
    }

    public class ServicioPrecioDto
    {
        public Guid ServicioId { get; set; }
        public decimal Precio { get; set; }
    }
}
