using System;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Direccion
    {
        public string Calle { get; }
        public string Ciudad { get; }
        public string CodigoPostal { get; }
        public string TextoCompleto { get; }
        
        private Direccion(string calle, string ciudad, string codigoPostal, string textoCompleto)
        {
            Calle = calle;
            Ciudad = ciudad;
            CodigoPostal = codigoPostal;
            TextoCompleto = textoCompleto;
        }
        
        public static Direccion Create(string textoCompleto)
        {
            if (string.IsNullOrWhiteSpace(textoCompleto))
                throw new ArgumentException("La dirección no puede estar vacía", nameof(textoCompleto));
            
            return new Direccion(
                textoCompleto, 
                string.Empty,  
                string.Empty,  
                textoCompleto
            );
        }
        
        public static Direccion Create(string calle, string ciudad, string codigoPostal)
        {
            if (string.IsNullOrWhiteSpace(calle))
                throw new ArgumentException("La calle no puede estar vacía", nameof(calle));
                
            if (string.IsNullOrWhiteSpace(ciudad))
                throw new ArgumentException("La ciudad no puede estar vacía", nameof(ciudad));
                
            if (!System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{5}$"))
                throw new ArgumentException("Formato de código postal inválido", nameof(codigoPostal));
                
            string textoCompleto = $"{calle}, {ciudad}, {codigoPostal}";
            
            return new Direccion(calle, ciudad, codigoPostal, textoCompleto);
        }
        
        public override string ToString() => TextoCompleto;
    }