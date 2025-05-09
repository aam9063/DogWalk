using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    public class AddItemCarritoDto
{
    public Guid ArticuloId { get; set; }
    public int Cantidad { get; set; } = 1;
}
}
