using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Paseadores;

/// <summary>
/// DTO para representar un paseador en el mapa.
/// </summary>
public class PaseadorMapDto
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
    public decimal PrecioBase { get; set; } // Precio más económico para mostrar
    // Etiquetas para mostrar en la interfaz
    public List<string> Etiquetas { get; set; } = new List<string>();
}
