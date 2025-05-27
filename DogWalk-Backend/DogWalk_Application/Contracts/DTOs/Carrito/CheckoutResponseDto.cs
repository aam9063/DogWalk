using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    /// <summary>
    /// DTO para representar la respuesta de la finalización de una compra.
    /// </summary>
    public class CheckoutResponseDto
    {
        public bool Success { get; set; }
        public string RedirectUrl { get; set; }
        public Guid? FacturaId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
