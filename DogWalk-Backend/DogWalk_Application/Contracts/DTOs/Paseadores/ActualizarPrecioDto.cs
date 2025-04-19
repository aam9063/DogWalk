using System;
using System.ComponentModel.DataAnnotations;

namespace DogWalk_Application.Contracts.DTOs.Paseadores;

public class ActualizarPrecioDto
{
    [Required]
    public Guid ServicioId { get; set; }
    
    [Required]
    [Range(0.01, 1000)]
    public decimal Precio { get; set; }
}
