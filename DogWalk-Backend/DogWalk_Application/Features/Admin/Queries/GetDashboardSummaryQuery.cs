using DogWalk_Application.Contracts.DTOs.Admin;
using MediatR;

namespace DogWalk_Application.Features.Admin.Queries
{
    public record GetDashboardSummaryQuery() : IRequest<DashboardSummaryDto>;
}