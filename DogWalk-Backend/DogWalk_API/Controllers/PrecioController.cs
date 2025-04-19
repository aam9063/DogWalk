// DogWalk_API/Controllers/PrecioController.cs
using DogWalk_Application.Contracts.DTOs.Precios;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrecioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrecioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Obtener precios de un paseador por ID
        [HttpGet("paseador/{paseadorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPreciosByPaseador(Guid paseadorId)
        {
            try
            {
                var paseador = await _unitOfWork.Paseadores.GetByIdAsync(paseadorId);
                if (paseador == null)
                {
                    return NotFound(new { message = "Paseador no encontrado" });
                }
                
                var precios = paseador.Precios.Select(p => new PrecioDto
                {
                    Id = p.Id,
                    PaseadorId = p.PaseadorId,
                    ServicioId = p.ServicioId,
                    NombreServicio = p.Servicio.Nombre,
                    DescripcionServicio = p.Servicio.Descripcion,
                    Precio = p.Valor.Cantidad
                }).ToList();
                
                return Ok(precios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al obtener precios: {ex.Message}" });
            }
        }
    }
}