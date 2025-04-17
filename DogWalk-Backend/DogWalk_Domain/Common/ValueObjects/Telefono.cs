using System;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Telefono
    {
        public string Numero { get; }
        
        private Telefono(string numero)
        {
            Numero = numero;
        }
        
        public static Telefono Create(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                throw new ArgumentException("El teléfono no puede estar vacío", nameof(telefono));
                
            // Validar formato: 9 dígitos
            if (!System.Text.RegularExpressions.Regex.IsMatch(telefono, @"^\d{9}$"))
                throw new ArgumentException("Formato de teléfono inválido, debe contener 9 dígitos", nameof(telefono));
                
            return new Telefono(telefono);
        }
        
        public override string ToString() => Numero;
    }
