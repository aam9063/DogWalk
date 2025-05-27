using DogWalk_Application.Contracts.DTOs.Admin;
using MediatR;

namespace DogWalk_Application.Features.Admin.Queries
{
    /// <summary>
    /// Consulta para obtener el resumen del dashboard.
    /// </summary>
    public record GetDashboardSummaryQuery() : IRequest<DashboardSummaryDto>;
}