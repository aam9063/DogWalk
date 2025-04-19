using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DogWalk_Application.Contracts.DTOs.Auth
{
    public class RegisterPaseadorDto
    {
        [Required]
        public string Dni { get; set; }
        
        [Required]
        public string Nombre { get; set; }
        
        [Required]
        public string Apellido { get; set; }
        
        [Required]
        public string Direccion { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        
        [Required]
        public string Telefono { get; set; }
        
        [Required]
        public double Latitud { get; set; }
        
        [Required]
        public double Longitud { get; set; }
        
        // Lista de servicios y precios
        public List<ServicioPrecioDto> Servicios { get; set; } = new List<ServicioPrecioDto>();
    }

    public class ServicioPrecioDto
    {
        public Guid ServicioId { get; set; }
        public decimal Precio { get; set; }
    }
}
