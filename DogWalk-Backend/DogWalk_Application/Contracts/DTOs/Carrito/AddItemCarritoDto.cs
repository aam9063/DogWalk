using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    public class AddItemCarritoDto
    {
        public string TipoItem { get; set; } // "Articulo" o "Servicio"
        public Guid ItemId { get; set; }
        public int Cantidad { get; set; } = 1;
    }
}
