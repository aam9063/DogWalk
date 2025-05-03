using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Articulos;

public class ArticuloDetailsDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Categoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public List<string> Imagenes { get; set; } = new List<string>();
}
