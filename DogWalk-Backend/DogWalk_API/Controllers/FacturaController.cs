using DogWalk_Application.Services;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using DogWalk_Application.Contracts.DTOs.Facturas;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DogWalk_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FacturaController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFacturaPdfService _facturaPdfService;
    private readonly ILogger<FacturaController> _logger;

    public FacturaController(
        IUnitOfWork unitOfWork,
        IFacturaPdfService facturaPdfService,
        ILogger<FacturaController> logger)
    {
        _unitOfWork = unitOfWork;
        _facturaPdfService = facturaPdfService;
        _logger = logger;
    }

    // Nuevo endpoint para obtener todas las facturas del usuario
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FacturaResumenDto>>> GetFacturasUsuario()
    {
        try
        {
            _logger.LogInformation("Obteniendo facturas del usuario");

            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdStr) || !Guid.TryParse(usuarioIdStr, out Guid usuarioId))
            {
                _logger.LogWarning("ID de usuario no válido o no encontrado en el token");
                return Unauthorized(new { error = "Usuario no identificado correctamente" });
            }

            var facturas = await _unitOfWork.Facturas.GetByUsuarioIdAsync(usuarioId);

            var facturasDto = facturas.Select(f => new FacturaResumenDto
            {
                Id = f.Id,
                FechaFactura = f.FechaFactura,
                Total = f.Total.Cantidad,
                CantidadItems = f.Detalles.Count,
                Estado = "Completada"
            }).OrderByDescending(f => f.FechaFactura)
            .ToList();

            return Ok(facturasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las facturas del usuario");
            return StatusCode(500, new { error = "Error al obtener las facturas", detalle = ex.Message });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DescargarFacturaPdf(Guid id)
    {
        try
        {
            _logger.LogInformation($"Iniciando descarga de factura PDF para ID: {id}");

            // Obtener el ID del usuario autenticado
            var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdStr) || !Guid.TryParse(usuarioIdStr, out Guid usuarioId))
            {
                _logger.LogWarning("ID de usuario no válido o no encontrado en el token");
                return Unauthorized(new { error = "Usuario no identificado correctamente" });
            }

            // Obtener la factura con todos sus detalles
            var factura = await _unitOfWork.Facturas.GetByIdAsync(id);
            
            if (factura == null)
            {
                _logger.LogWarning($"Factura no encontrada: {id}");
                return NotFound(new { error = "Factura no encontrada" });
            }

            // Verificar que la factura pertenece al usuario
            if (factura.UsuarioId != usuarioId)
            {
                _logger.LogWarning($"Intento de acceso no autorizado a factura {id} por usuario {usuarioId}");
                return Forbid();
            }

            try
            {
                _logger.LogInformation($"Generando PDF para factura {id}");
                var pdfBytes = await _facturaPdfService.GenerarPdfFactura(factura);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    _logger.LogError($"PDF generado está vacío para factura {id}");
                    return StatusCode(500, new { error = "Error al generar el PDF: documento vacío" });
                }

                _logger.LogInformation($"PDF generado exitosamente para factura {id}. Tamaño: {pdfBytes.Length} bytes");
                
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"factura-{factura.Id}.pdf",
                    true
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar PDF para factura {id}");
                return StatusCode(500, new { error = "Error al generar el PDF", detalle = ex.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado al procesar la descarga de factura");
            return StatusCode(500, new { error = "Error interno del servidor", detalle = ex.Message });
        }
    }
}