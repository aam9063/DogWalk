using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Articulos;

public class UpdateArticuloDto
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Categoria { get; set; } // Enum CategoriaArticulo como int
    public List<string> Imagenes { get; set; } = new List<string>();
}
