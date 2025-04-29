using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Services.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Common.Enums;
using DogWalk_Application.Contracts.DTOs.Carrito;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    [Route("api/stripe-webhook")]
    [ApiController]
    public class StripeFinalController : ControllerBase
    {
        private static IUnitOfWork GetUnitOfWork(HttpContext context) => 
            context.RequestServices.GetRequiredService<IUnitOfWork>();
            
        private static StripeService GetStripeService(HttpContext context) => 
            context.RequestServices.GetRequiredService<StripeService>();
            
        private static ILogger<StripeFinalController> GetLogger(HttpContext context) =>
            context.RequestServices.GetRequiredService<ILogger<StripeFinalController>>();

        private static IConfiguration GetConfiguration(HttpContext context) =>
            context.RequestServices.GetRequiredService<IConfiguration>();

        [HttpPost]
        public async Task<IActionResult> HandleStripeEvent()
        {
            var logger = GetLogger(HttpContext);
            logger.LogInformation("Recibiendo solicitud de webhook de Stripe");
            
            var configuration = GetConfiguration(HttpContext);
            var webhookSecret = configuration["Stripe:WebhookSecret"];
            logger.LogInformation($"WebhookSecret configurado: {webhookSecret}");
            
            try
            {
                // Obtener servicios bajo demanda
                var unitOfWork = GetUnitOfWork(HttpContext);
                var stripeService = GetStripeService(HttpContext);
                
                // Lee el cuerpo de la solicitud de manera que pueda ser leído múltiples veces
                Request.EnableBuffering();
                using var reader = new StreamReader(Request.Body, leaveOpen: true);
                var json = await reader.ReadToEndAsync();
                
                // Restaura la posición del stream para que pueda ser leído de nuevo si es necesario
                Request.Body.Position = 0;
                
                // Registra información para depuración
                logger.LogInformation($"Payload recibido: {json}");
                var signatureHeader = Request.Headers["Stripe-Signature"];
                logger.LogInformation($"Encabezado de firma: {signatureHeader}");
                
                if (string.IsNullOrEmpty(json))
                {
                    logger.LogWarning("Payload vacío");
                    return BadRequest(new { error = "No se recibió contenido" });
                }
                
                // Verificar firma
                var stripeEvent = stripeService.ProcessWebhookEvent(
                    json, 
                    signatureHeader
                );
                
                logger.LogInformation($"Evento procesado: {stripeEvent.Type}");
                
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    
                    if (session?.Metadata != null)
                    {
                        logger.LogInformation($"Metadatos: {string.Join(", ", session.Metadata.Select(m => $"{m.Key}={m.Value}"))}");
                        
                        if (session.Metadata.TryGetValue("FacturaId", out string facturaIdStr) && 
                            Guid.TryParse(facturaIdStr, out Guid facturaId))
                        {
                            logger.LogInformation($"Factura ID encontrada: {facturaId}");
                            
                            var factura = await unitOfWork.Facturas.GetByIdAsync(facturaId);
                            if (factura != null)
                            {
                                // Añade un método a tu entidad Factura para marcarla como pagada
                                // Por ejemplo:
                                // factura.MarcarComoPagada();
                                
                                await unitOfWork.SaveChangesAsync();
                                logger.LogInformation($"Factura {facturaId} marcada como pagada exitosamente");
                            }
                            else
                            {
                                logger.LogWarning($"Factura no encontrada: {facturaId}");
                            }
                        }
                        else
                        {
                            logger.LogWarning("ID de factura no encontrado en los metadatos");
                        }
                    }
                    else
                    {
                        logger.LogWarning("Sesión sin metadatos");
                    }
                }
                
                return Ok(new { received = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar webhook");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("iniciar-pago")]
        [Authorize]
        public async Task<IActionResult> IniciarPago([FromBody] CheckoutDto checkout)
        {
            try
            {
                // Obtener servicios bajo demanda
                var unitOfWork = GetUnitOfWork(HttpContext);
                var stripeService = GetStripeService(HttpContext);
                var logger = GetLogger(HttpContext);
                
                // Obtener ID del usuario desde los claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid usuarioId))
                {
                    return Unauthorized(new { error = "Usuario no identificado" });
                }
                
                // Obtener usuario y su carrito
                var usuario = await unitOfWork.Usuarios.GetByIdAsync(usuarioId);
                if (usuario == null) return NotFound(new { error = "Usuario no encontrado" });
                
                if (!usuario.Carrito.Any())
                    return BadRequest(new { error = "El carrito está vacío" });
                
                // Crear factura
                var factura = new Factura(
                    Guid.NewGuid(), 
                    usuarioId, 
                    MetodoPago.Stripe
                );
                
                // Agregar ítems del carrito a la factura
                foreach(var item in usuario.Carrito)
                {
                    factura.AgregarDetalle(new DetalleFactura(
                        Guid.NewGuid(),
                        factura.Id,
                        item.TipoItem,
                        item.ItemId,
                        item.Cantidad,
                        item.PrecioUnitario
                    ));
                }
                
                // Guardar factura
                await unitOfWork.Facturas.AddAsync(factura);
                
                // Crear sesión de Stripe
                var successUrl = "http://localhost:5173/checkout/success?session_id={CHECKOUT_SESSION_ID}";
                var cancelUrl = "http://localhost:5173/checkout/cancel";
                
                var stripeSessionUrl = await stripeService.CreateCheckoutSession(
                    factura,
                    successUrl,
                    cancelUrl
                );
                
                // Opcional: Vaciar carrito o marcarlo como en proceso de pago
                usuario.VaciarCarrito();
                
                await unitOfWork.SaveChangesAsync();
                
                return Ok(new { url = stripeSessionUrl, facturaId = factura.Id });
            }
            catch (Exception ex)
            {
                var logger = GetLogger(HttpContext);
                logger.LogError(ex, "Error al iniciar pago");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
