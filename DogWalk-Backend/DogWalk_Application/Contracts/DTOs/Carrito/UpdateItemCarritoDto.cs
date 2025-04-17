using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    public class UpdateItemCarritoDto
    {
        public Guid ItemCarritoId { get; set; }
        public int Cantidad { get; set; }
    }
}
