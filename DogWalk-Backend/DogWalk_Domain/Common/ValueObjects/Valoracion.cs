using System;

namespace DogWalk_Domain.Common.ValueObjects;

 public sealed record Valoracion
    {
        public int Puntuacion { get; }
        
        private Valoracion(int puntuacion)
        {
            Puntuacion = puntuacion;
        }
        
        public static Valoracion Create(int puntuacion)
        {
            if (puntuacion < 1 || puntuacion > 5)
                throw new ArgumentException("La valoración debe estar entre 1 y 5", nameof(puntuacion));
                
            return new Valoracion(puntuacion);
        }
        
        public override string ToString() => Puntuacion.ToString();
    }
    