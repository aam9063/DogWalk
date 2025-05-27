using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Articulos;

/// <summary>
/// DTO para crear un nuevo artículo.
/// </summary>
public class CreateArticuloDto
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int Categoria { get; set; } // Enum CategoriaArticulo como int
    public List<string> Imagenes { get; set; } = new List<string>();
}
