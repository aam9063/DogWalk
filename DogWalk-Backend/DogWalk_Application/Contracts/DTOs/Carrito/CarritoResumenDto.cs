using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    /// <summary>
    /// DTO para representar un resumen del carrito.
    /// </summary>
    public class CarritoResumenDto
    {
        public int CantidadItems { get; set; }
        public decimal Total { get; set; }
    }
}
