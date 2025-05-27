using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    /// <summary>
    /// DTO para agregar un artículo al carrito.
    /// </summary>
    public class AddItemCarritoDto
    {
        public Guid ArticuloId { get; set; }
        public int Cantidad { get; set; } = 1;
    }
}
