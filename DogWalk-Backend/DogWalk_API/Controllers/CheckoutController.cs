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

/// <summary>
/// Controlador para procesar el pago con Stripe.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeService _stripeService;
    private readonly ILogger<CheckoutController> _logger;

    /// <summary>
    /// Constructor del controlador de checkout.
    /// </summary>
    /// <param name="unitOfWork">Unidad de trabajo</param>
    /// <param name="stripeService">Servicio de Stripe</param>
    /// <param name="logger">Logger</param>
    public CheckoutController(
        IUnitOfWork unitOfWork,
        StripeService stripeService,
        ILogger<CheckoutController> logger)
    {
        _unitOfWork = unitOfWork;
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// Procesa el checkout.
    /// </summary>
    /// <returns>Resultado de la operación</returns>
    /// <response code="200">Si el checkout se procesó correctamente</response>
    /// <response code="400">Si el checkout no se procesó correctamente</response>
    /// <response code="500">Si ocurre un error al procesar el checkout</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CheckoutResponseDto>> ProcesarCheckout()
    {
        try
        {
            // 1. Obtener usuario y carrito
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(userId);
            
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            if (!usuario.Carrito.Any())
                return BadRequest("El carrito está vacío");

            // 2. Crear la factura
            var factura = new Factura(Guid.NewGuid(), usuario.Id, MetodoPago.Stripe);

            // 3. Procesar cada item del carrito y mantener una referencia a los artículos
            var articulosDict = new Dictionary<Guid, Articulo>();

            foreach (var item in usuario.Carrito)
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ArticuloId);
                if (articulo == null)
                {
                    _logger.LogError($"Artículo no encontrado: {item.ArticuloId}");
                    return BadRequest($"Artículo no encontrado en el carrito");
                }

                if (articulo.Stock < item.Cantidad)
                {
                    return BadRequest($"Stock insuficiente para {articulo.Nombre}");
                }

                // Guardar el artículo en el diccionario
                articulosDict[articulo.Id] = articulo;

                var detalle = new DetalleFactura(
                    Guid.NewGuid(),
                    factura.Id,
                    articulo.Id,
                    item.Cantidad,
                    item.PrecioUnitario
                );

                factura.AgregarDetalle(detalle);
                articulo.ReducirStock(item.Cantidad);
            }

            try
            {
                var successUrl = "http://localhost:5173/checkout/success?session_id={CHECKOUT_SESSION_ID}";
                var cancelUrl = "http://localhost:5173/checkout/cancel";

                // Crear una factura temporal con los artículos para Stripe
                var facturaParaStripe = new Factura(factura.Id, factura.UsuarioId, factura.MetodoPago);
                foreach (var detalle in factura.Detalles)
                {
                    var detalleConArticulo = new DetalleFactura(
                        detalle.Id,
                        facturaParaStripe.Id,
                        detalle.ArticuloId,
                        detalle.Cantidad,
                        detalle.PrecioUnitario
                    );
                    // Asignar el artículo desde nuestro diccionario
                    detalleConArticulo.GetType().GetProperty("Articulo")?.SetValue(detalleConArticulo, articulosDict[detalle.ArticuloId]);
                    facturaParaStripe.AgregarDetalle(detalleConArticulo);
                }

                var stripeSessionUrl = await _stripeService.CreateCheckoutSession(
                    facturaParaStripe,
                    successUrl,
                    cancelUrl
                );

                // Guardar la factura original
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
                _logger.LogError(ex, "Error al procesar el pago con Stripe");
                return BadRequest(new CheckoutResponseDto
                {
                    Success = false,
                    ErrorMessage = $"Error al procesar el pago: {ex.Message}"
                });
            }
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
