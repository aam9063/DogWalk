namespace DogWalk_Application.Contracts.DTOs.Paseadores
{
    public class PaseadorProfileDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string FotoPerfil { get; set; }
        public decimal ValoracionGeneral { get; set; }
        public int CantidadValoraciones { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public List<ServicioPrecioSimpleDto> Servicios { get; set; } = new List<ServicioPrecioSimpleDto>();
    }

    public class ServicioPrecioSimpleDto
    {
        public Guid ServicioId { get; set; }
        public string NombreServicio { get; set; }
        public decimal Precio { get; set; }
    }
}