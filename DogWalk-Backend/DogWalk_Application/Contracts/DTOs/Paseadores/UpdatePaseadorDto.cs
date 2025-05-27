namespace DogWalk_Application.Contracts.DTOs.Paseadores
{
    /// <summary>
    /// DTO para actualizar un paseador.
    /// </summary>
    public class UpdatePaseadorDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}