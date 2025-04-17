using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            // Implementación para obtener datos del dashboard
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var paseadores = await _unitOfWork.Paseadores.GetAllAsync();
            var reservas = await _unitOfWork.Reservas.GetAllAsync();

            return new DashboardSummaryDto
            {
                TotalUsuarios = usuarios.Count(),
                TotalPaseadores = paseadores.Count(),
                TotalReservas = reservas.Count(),
                ReservasCompletadas = reservas.Count(r => r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Completada),
                ReservasPendientes = reservas.Count(r => r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Pendiente),
                ReservasCanceladas = reservas.Count(r => r.Estado == DogWalk_Domain.Common.Enums.EstadoReserva.Cancelada),
                PaseadoresMasReservados = paseadores.GroupBy(p => p.Nombre)
                    .OrderByDescending(g => g.Count())
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                    .ToList()
                
            };
        }
    }
}