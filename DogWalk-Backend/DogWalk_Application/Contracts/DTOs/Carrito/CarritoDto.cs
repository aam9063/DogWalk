using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    /// <summary>
    /// DTO para representar un carrito de compras.
    /// </summary>
    public class CarritoDto
    {
        public Guid UsuarioId { get; set; }
        public List<ItemCarritoDto> Items { get; set; } = new();
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
    }
}
