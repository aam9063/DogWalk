using System;

namespace DogWalk_Domain.Common.ValueObjects;

public sealed record Coordenadas
    {
        public double Latitud { get; }
        public double Longitud { get; }
        
        private Coordenadas(double latitud, double longitud)
        {
            Latitud = latitud;
            Longitud = longitud;
        }
        
        public static Coordenadas Create(double latitud, double longitud)
        {
            if (latitud < -90 || latitud > 90)
                throw new ArgumentException("Latitud fuera de rango válido (-90 a 90)", nameof(latitud));
                
            if (longitud < -180 || longitud > 180)
                throw new ArgumentException("Longitud fuera de rango válido (-180 a 180)", nameof(longitud));
                
            return new Coordenadas(latitud, longitud);
        }
        
        public override string ToString() => $"{Latitud}, {Longitud}";
    }
