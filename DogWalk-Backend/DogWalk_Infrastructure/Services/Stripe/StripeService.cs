using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalk_Infrastructure.Services.Stripe;

public class StripeService
{
    private readonly StripeOptions _options;
    
    public StripeService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        StripeConfiguration.ApiKey = _options.SecretKey;
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
        var lineItems = new List<SessionLineItemOptions>();
        
        foreach (var detalle in factura.Detalles)
        {
            string descripcion = detalle.TipoItem == DogWalk_Domain.Common.Enums.TipoItem.Articulo 
                ? "Artículo" 
                : "Servicio";
            
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(detalle.PrecioUnitario.Cantidad * 100), // Stripe usa centavos
                    Currency = _options.Currency,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"{descripcion} #{detalle.ItemId}",
                        Description = $"{descripcion} de DogWalk",
                    }
                },
                Quantity = detalle.Cantidad,
            });
        }
        
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = $"{successUrl}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = cancelUrl,
            ClientReferenceId = factura.Id.ToString(),
            CustomerEmail = factura.Usuario.Email.Valor,
            Metadata = new Dictionary<string, string>
            {
                { "FacturaId", factura.Id.ToString() },
                { "UsuarioId", factura.UsuarioId.ToString() }
            }
        };
        
        var service = new SessionService();
        var session = await service.CreateAsync(options);
        
        return session.Url;
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
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException(nameof(json), "El JSON no puede ser nulo o vacío");
        
        if (string.IsNullOrEmpty(signatureHeader))
            throw new ArgumentNullException(nameof(signatureHeader), "El encabezado de firma no puede ser nulo o vacío");
        
        if (string.IsNullOrEmpty(_options.WebhookSecret))
            throw new InvalidOperationException("WebhookSecret no está configurado en las opciones de Stripe");
        
        try
        {
            return EventUtility.ConstructEvent(
                json,
                signatureHeader,
                _options.WebhookSecret
            );
        }
        catch (StripeException ex)
        {
            throw new Exception($"Error al procesar webhook de Stripe: {ex.Message}", ex);
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
