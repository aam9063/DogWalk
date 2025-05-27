using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalk_Infrastructure.Services.Stripe;

/// <summary>
/// Servicio para interactuar con Stripe.
/// </summary>
public class StripeService
{
    private readonly string _apiKey;
    private readonly string _webhookSecret;
    private readonly ILogger<StripeService> _logger;

    /// <summary>
    /// Constructor del servicio Stripe.
    /// </summary>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="logger">Logger para registrar mensajes.</param>
    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _apiKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey no está configurado");
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? throw new ArgumentNullException("Stripe:WebhookSecret no está configurado");
        _logger = logger;
        StripeConfiguration.ApiKey = _apiKey;
    }
    
    /// <summary>
    /// Crea una sesión de pago en Stripe para una factura
    /// </summary>
    /// <param name="factura">La factura a pagar</param>
    /// <param name="successUrl">URL de retorno en caso de éxito</param>
    /// <param name="cancelUrl">URL de retorno en caso de cancelación</param>
    /// <returns>URL de la sesión de pago de Stripe</returns>
    public async Task<string> CreateCheckoutSession(Factura factura, string successUrl, string cancelUrl)
    {
        try
        {
            _logger.LogInformation($"Creando sesión de checkout para factura {factura.Id}");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "FacturaId", factura.Id.ToString() }
                }
            };

            foreach (var detalle in factura.Detalles)
            {
                _logger.LogInformation($"Procesando detalle {detalle.Id} para artículo {detalle.ArticuloId}");

                var lineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur", // o detalle.PrecioUnitario.Moneda.ToLower()
                        UnitAmount = (long)(detalle.PrecioUnitario.Cantidad * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = detalle.Articulo?.Nombre ?? "Producto",
                            Description = detalle.Articulo?.Descripcion ?? "Sin descripción"
                        }
                    },
                    Quantity = detalle.Cantidad
                };

                options.LineItems.Add(lineItem);
            }

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            
            _logger.LogInformation($"Sesión de checkout creada exitosamente: {session.Id}");
            return session.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sesión de checkout");
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si un pago ha sido completado
    /// </summary>
    /// <param name="sessionId">ID de la sesión de Stripe</param>
    /// <returns>True si el pago ha sido completado</returns>
    public async Task<bool> VerifyPayment(string sessionId)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId);
        
        // Verificar si la sesión se ha pagado
        return session.PaymentStatus == "paid";
    }
    
    /// <summary>
    /// Procesa un evento de webhook de Stripe
    /// </summary>
    /// <param name="json">JSON del evento</param>
    /// <param name="signatureHeader">Firma del evento</param>
    /// <returns>El evento procesado</returns>
    public Event ProcessWebhookEvent(string json, string signatureHeader)
    {
        try
        {
            return EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _webhookSecret
            );
        }
        catch (StripeException e)
        {
            throw new Exception($"Error al procesar webhook de Stripe: {e.Message}");
        }
    }
    
    /// <summary>
    /// Crea un cliente en Stripe
    /// </summary>
    /// <param name="email">Email del cliente</param>
    /// <param name="nombre">Nombre del cliente</param>
    /// <param name="metadata">Metadatos adicionales</param>
    /// <returns>ID del cliente en Stripe</returns>
    public async Task<string> CreateCustomer(string email, string nombre, Dictionary<string, string> metadata = null)
    {
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = nombre,
            Metadata = metadata
        };
        
        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        
        return customer.Id;
    }
    
    /// <summary>
    /// Crea un reembolso para un pago
    /// </summary>
    /// <param name="paymentIntentId">ID del PaymentIntent</param>
    /// <param name="amount">Cantidad a reembolsar (en centavos)</param>
    /// <returns>El reembolso creado</returns>
    public async Task<Refund> CreateRefund(string paymentIntentId, long? amount = null)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
            Amount = amount
        };
        
        var service = new RefundService();
        return await service.CreateAsync(options);
    }
    
    /// <summary>
    /// Obtiene todos los métodos de pago de un cliente
    /// </summary>
    /// <param name="customerId">ID del cliente en Stripe</param>
    /// <returns>Lista de métodos de pago</returns>
    public async Task<IEnumerable<PaymentMethod>> GetCustomerPaymentMethods(string customerId)
    {
        var options = new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card"
        };
        
        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(options);
        
        return paymentMethods.Data;
    }
}
