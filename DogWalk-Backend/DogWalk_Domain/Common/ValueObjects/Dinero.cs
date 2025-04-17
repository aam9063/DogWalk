using System;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Dinero
    {
        public decimal Cantidad { get; }
        public string Moneda { get; } = "EUR";
        
        private Dinero(decimal cantidad, string moneda)
        {
            Cantidad = cantidad;
            Moneda = moneda;
        }
        
        public static Dinero Create(decimal cantidad, string moneda = "EUR")
        {
            if (cantidad < 0)
                throw new ArgumentException("La cantidad no puede ser negativa", nameof(cantidad));
            
            // Redondeamos a 2 decimales
            cantidad = Math.Round(cantidad, 2);
            
            return new Dinero(cantidad, moneda);
        }
        
        public static Dinero operator +(Dinero a, Dinero b)
        {
            if (a.Moneda != b.Moneda)
                throw new InvalidOperationException("No se pueden sumar cantidades con monedas diferentes");
                
            return Create(a.Cantidad + b.Cantidad, a.Moneda);
        }
        
        public static Dinero operator *(Dinero a, int cantidad)
        {
            return Create(a.Cantidad * cantidad, a.Moneda);
        }
        
        public override string ToString() => $"{Cantidad} {Moneda}";
    }