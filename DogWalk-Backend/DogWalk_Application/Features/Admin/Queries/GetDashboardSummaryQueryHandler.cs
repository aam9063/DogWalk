using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Application.Features.Admin.Queries
{
    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDashboardSummaryQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            // Obtener datos básicos
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
            var reservas = await _unitOfWork.Reservas.GetAllAsync();
            var facturas = await _unitOfWork.Facturas.GetAllAsync();
            var servicios = await _unitOfWork.Servicios.GetAllAsync();

            // Calcular total de artículos vendidos
            var totalArticulosVendidos = facturas
                .Where(f => f.Detalles != null)
                .SelectMany(f => f.Detalles)
                .Sum(d => d.Cantidad);

            // Calcular ingresos totales y mensuales
            var ingresosTotales = facturas.Sum(f => f.Total.Cantidad);
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ingresosMensuales = facturas
                .Where(f => f.FechaFactura >= inicioMes)
                .Sum(f => f.Total.Cantidad);

            // Calcular servicios más populares
            var serviciosMasPopulares = reservas
                .Where(r => r.ServicioId != Guid.Empty)
                .GroupBy(r => r.ServicioId)
                .Select(g => new { ServicioId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var serviciosDict = servicios.ToDictionary(s => s.Id, s => s.Nombre);

            var serviciosMasPopularesConNombres = serviciosMasPopulares
                .Select(s => new KeyValuePair<string, int>(
                    serviciosDict.GetValueOrDefault(s.ServicioId, "Servicio Desconocido"),
                    s.Count))
                .ToList();

            return new DashboardSummaryDto
            {
                TotalUsuarios = usuarios.Count(),
                TotalPaseadores = paseadores.Count(),
                TotalReservas = reservas.Count(),
                ReservasCompletadas = reservas.Count(r => r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Completada),
                ReservasPendientes = reservas.Count(r => r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Pendiente),
                ReservasCanceladas = reservas.Count(r => r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Cancelada),
                IngresosTotales = ingresosTotales,
                IngresosMensuales = ingresosMensuales,
                TotalArticulosVendidos = totalArticulosVendidos,
                ServiciosMasPopulares = serviciosMasPopularesConNombres,
                PaseadoresMasReservados = paseadores
                    .GroupBy(p => p.Nombre)
                    .OrderByDescending(g => g.Count())
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                    .ToList()
            };
        }
    }
}