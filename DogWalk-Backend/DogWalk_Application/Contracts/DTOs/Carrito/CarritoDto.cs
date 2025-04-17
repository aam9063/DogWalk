using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    public class CarritoDto
    {
        public Guid UsuarioId { get; set; }
        public List<ItemCarritoDto> Items { get; set; } = new List<ItemCarritoDto>();
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }
    }
}
