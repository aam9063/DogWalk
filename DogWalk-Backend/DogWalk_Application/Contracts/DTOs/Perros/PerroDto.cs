namespace DogWalk_Application.Contracts.DTOs.Perros
{
    /// <summary>
    /// DTO para representar un perro.
    /// </summary>
    public class PerroDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public int Edad { get; set; }
        public string GpsUbicacion { get; set; }
        public decimal ValoracionPromedio { get; set; }
        public string UrlFotoPrincipal { get; set; }
    }
}