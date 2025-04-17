using System;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Dni
    {
        public string Valor { get; }
        
        private Dni(string valor)
        {
            Valor = valor;
        }
        
        public static Dni Create(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException("El DNI no puede estar vacío", nameof(dni));
                
            // Validar formato con regex: 8 dígitos + 1 letra mayúscula
            if (!System.Text.RegularExpressions.Regex.IsMatch(dni, @"^\d{8}[A-Z]$"))
                throw new ArgumentException("Formato de DNI inválido", nameof(dni));
                
            return new Dni(dni);
        }
        
        public override string ToString() => Valor;
    }
