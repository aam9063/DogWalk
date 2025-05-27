using System;

namespace DogWalk_Domain.Common.ValueObjects;

/// <summary>
/// Representa un conjunto de coordenadas geográficas.
/// </summary>
public sealed record Coordenadas
{
    /// <summary>
    /// Obtiene la latitud de las coordenadas.
    /// </summary>
    public double Latitud { get; }

    /// <summary>
    /// Obtiene la longitud de las coordenadas.
    /// </summary>
    public double Longitud { get; }

    private Coordenadas(double latitud, double longitud)
    {
        Latitud = latitud;
        Longitud = longitud;
    }

    /// <summary>
    /// Crea un nuevo conjunto de coordenadas.
    /// </summary>
    /// <param name="latitud">La latitud de las coordenadas.</param>
    /// <param name="longitud">La longitud de las coordenadas.</param>
    /// <returns>Un nuevo conjunto de coordenadas.</returns>
    public static Coordenadas Create(double latitud, double longitud)
    {
        if (latitud < -90 || latitud > 90)
            throw new ArgumentException("Latitud fuera de rango válido (-90 a 90)", nameof(latitud));

        if (longitud < -180 || longitud > 180)
            throw new ArgumentException("Longitud fuera de rango válido (-180 a 180)", nameof(longitud));

        return new Coordenadas(latitud, longitud);
    }

    /// <summary>
    /// Convierte las coordenadas a una cadena de texto.
    /// </summary>
    /// <returns>Una cadena de texto que representa las coordenadas.</returns>
    public override string ToString() => $"{Latitud}, {Longitud}";
}
