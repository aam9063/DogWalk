using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    /// <summary>
    /// DTO para actualizar la cantidad de un artículo en el carrito.
    /// </summary>
    public class UpdateItemCarritoDto
    {
        public Guid ItemCarritoId { get; set; }
        public int Cantidad { get; set; }
    }
}
