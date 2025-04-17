using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    public class CheckoutResponseDto
    {
        public bool Success { get; set; }
        public string RedirectUrl { get; set; } // URL para redireccionar a la pasarela de pago
        public Guid? FacturaId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
