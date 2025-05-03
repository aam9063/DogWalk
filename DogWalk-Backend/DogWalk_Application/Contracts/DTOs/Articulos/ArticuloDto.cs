using System;

namespace DogWalk_Application.Contracts.DTOs.Articulos;

public class ArticuloDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; }
    public string ImagenPrincipal { get; set; }
    public DateTime FechaCreacion { get; set; }
}
