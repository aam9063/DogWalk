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

    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con los precios de los servicios.
    /// Incluye gestión de precios por paseador y por servicio.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PrecioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor del controlador de precios.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        public PrecioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene los precios de un paseador por su ID.
        /// </summary>
        /// <param name="paseadorId">ID del paseador</param>
        /// <returns>Lista de precios del paseador</returns>
        /// <response code="200">Retorna la lista de precios</response>
        /// <response code="404">Si el paseador no se encuentra</response>
        /// <response code="500">Si ocurre un error al obtener los precios</response>
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