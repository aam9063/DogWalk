public class UsuarioProfileDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string FotoPerfil { get; set; }
    public int CantidadPerros { get; set; }
    public int CantidadReservas { get; set; }
    public List<PerroProfileDto> Perros { get; set; } 
}

public class PerroProfileDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Raza { get; set; }
    public int Edad { get; set; }
    public string GpsUbicacion { get; set; }
    public List<string> Fotos { get; set; } 
}