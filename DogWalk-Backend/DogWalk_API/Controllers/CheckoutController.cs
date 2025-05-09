using DogWalk_Application.Contracts.DTOs.Carrito;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Services.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace DogWalk_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeService _stripeService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        IUnitOfWork unitOfWork, 
        StripeService stripeService,
        ILogger<CheckoutController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CheckoutResponseDto>> ProcesarCheckout()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(userId);
            
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            if (!usuario.Carrito.Any())
                return BadRequest("El carrito está vacío");

            // Crear la factura
            var factura = new Factura(
                Guid.NewGuid(),
                usuario.Id,
                MetodoPago.Stripe
            );

            // Agregar los detalles de la factura
            foreach (var item in usuario.Carrito)
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ArticuloId);
                if (articulo == null)
                    return BadRequest($"Artículo no encontrado: {item.ArticuloId}");

                if (!articulo.ReducirStock(item.Cantidad))
                    return BadRequest($"No hay suficiente stock para {articulo.Nombre}");

                var detalle = new DetalleFactura(
                    Guid.NewGuid(),
                    factura.Id,
                    item.ArticuloId,
                    item.Cantidad,
                    item.PrecioUnitario
                );
                
                factura.AgregarDetalle(detalle);
            }

            // Crear sesión de Stripe
            var successUrl = "http://localhost:5173/checkout/success?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = "http://localhost:5173/checkout/cancel";
            
            var stripeSessionUrl = await _stripeService.CreateCheckoutSession(
                factura,
                successUrl,
                cancelUrl
            );

            // Guardar la factura
            await _unitOfWork.Facturas.AddAsync(factura);

            // Vaciar el carrito
            usuario.VaciarCarrito();
            
            // Guardar todos los cambios
            await _unitOfWork.SaveChangesAsync();

            return Ok(new CheckoutResponseDto
            {
                Success = true,
                RedirectUrl = stripeSessionUrl,
                FacturaId = factura.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el proceso de checkout");
            return BadRequest(new CheckoutResponseDto
            {
                Success = false,
                ErrorMessage = $"Error al procesar el checkout: {ex.Message}"
            });
        }
    }
}
