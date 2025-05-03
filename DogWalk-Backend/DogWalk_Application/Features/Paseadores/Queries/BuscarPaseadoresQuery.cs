using System;
using DogWalk_Application.Contracts.DTOs.Busqueda;
using DogWalk_Application.Contracts.DTOs.Paseadores;
using MediatR;

namespace DogWalk_Application.Features.Paseadores.Queries;

public class BuscarPaseadoresQuery : IRequest<ResultadoPaginadoDto<PaseadorMapDto>>
{
    public string CodigoPostal { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public DateTime? FechaRecogida { get; set; }
    public int? CantidadPerros { get; set; }
    public Guid? ServicioId { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public double? DistanciaMaxima { get; set; } = 10.0; // Por defecto 10 km
    public decimal? ValoracionMinima { get; set; }
    public int Pagina { get; set; } = 1;
    public int ElementosPorPagina { get; set; } = 10;
}
