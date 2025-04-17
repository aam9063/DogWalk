namespace DogWalk_Application.Contracts.DTOs.Paseadores
{
    public class PaseadorDto
    {
        public Guid Id { get; set; }
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string FotoPerfil { get; set; }
        public decimal ValoracionGeneral { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}