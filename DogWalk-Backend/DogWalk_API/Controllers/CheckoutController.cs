using DogWalk_Application.Contracts.DTOs.Carrito;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Services.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeService _stripeService;

    public CheckoutController(IUnitOfWork unitOfWork, StripeService stripeService)
    {
        _unitOfWork = unitOfWork;
        _stripeService = stripeService;
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        return Guid.Parse(userId);
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponseDto>> ProcesarCheckout([FromBody] CheckoutDto checkoutDto)
    {
        // 1. Obtener usuario y su carrito
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
        if (usuario == null)
            return NotFound("Usuario no encontrado");

        if (!usuario.Carrito.Any())
            return BadRequest("El carrito está vacío");

        try
        {
            // 2. Crear factura
            MetodoPago metodoPago;
            if (!Enum.TryParse(checkoutDto.MetodoPago, true, out metodoPago))
                return BadRequest("Método de pago no válido");

            var factura = new Factura(
                Guid.NewGuid(),
                usuario.Id,
                metodoPago
            );

            // 3. Agregar detalles a la factura
            foreach (var item in usuario.Carrito)
            {
                var detalle = new DetalleFactura(
                    Guid.NewGuid(),
                    factura.Id,
                    item.TipoItem,
                    item.ItemId,
                    item.Cantidad,
                    item.PrecioUnitario
                );
                
                factura.AgregarDetalle(detalle);
                
                // Si es un artículo, reducir stock
                if (item.TipoItem == TipoItem.Articulo)
                {
                    var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ItemId);
                    if (articulo != null)
                    {
                        if (!articulo.ReducirStock(item.Cantidad))
                            return BadRequest($"No hay suficiente stock para {item.ItemId}");
                    }
                }
            }

            // 4. Guardar factura en la base de datos
            await _unitOfWork.Facturas.AddAsync(factura);
            
            // 5. Iniciar transacción para asegurar consistencia
            await _unitOfWork.BeginTransactionAsync();
            
            // 6. Crear sesión de pago en Stripe
            var successUrl = "http://localhost:5173/checkout/success?session_id={CHECKOUT_SESSION_ID}"; // Ajusta a tu frontend
            var cancelUrl = "http://localhost:5173/checkout/cancel";
            
            var stripeSessionUrl = await _stripeService.CreateCheckoutSession(
                factura, 
                successUrl, 
                cancelUrl
            );
            
            // 7. Vaciar el carrito (opcional - puedes dejarlo hasta confirmar el pago)
            usuario.VaciarCarrito();
            
            // 8. Guardar cambios
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            // 9. Devolver URL de Stripe
            return Ok(new CheckoutResponseDto
            {
                Success = true,
                RedirectUrl = stripeSessionUrl,
                FacturaId = factura.Id
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new CheckoutResponseDto
            {
                Success = false,
                ErrorMessage = $"Error al procesar el checkout: {ex.Message}"
            });
        }
    }

    [HttpPost("test")]
    [AllowAnonymous]
    public async Task<ActionResult<CheckoutResponseDto>> ProcesarCheckoutTest([FromBody] CheckoutDto checkoutDto)
    {
        try
        {
            Guid usuarioId = Guid.Parse("65cbfc9f-65f2-4813-b88c-84dc4b7b8757");
            
            // Obtener usuario
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            // Si el carrito está vacío, agregar un ítem de prueba
            if (!usuario.Carrito.Any())
            {
                // Buscar un artículo existente en la base de datos
                var articulos = await _unitOfWork.Articulos.GetAllAsync();
                var articulo = articulos.FirstOrDefault();
                
                if (articulo != null)
                {
                    // Crear y agregar un ítem al carrito
                    var itemCarrito = new ItemCarrito(
                        Guid.NewGuid(),
                        usuario.Id,
                        TipoItem.Articulo,
                        articulo.Id,
                        1, // Cantidad
                        articulo.Precio // Precio del artículo
                    );
                    
                    usuario.AgregarItemCarrito(itemCarrito);
                    await _unitOfWork.SaveChangesAsync();
                    
                    // Log para depuración
                    Console.WriteLine($"Artículo agregado al carrito: {articulo.Id}");
                }
                else
                {
                    return BadRequest("No hay artículos disponibles para agregar al carrito");
                }
            }

            // 2. Crear factura
            MetodoPago metodoPago;
            if (!Enum.TryParse(checkoutDto.MetodoPago, true, out metodoPago))
                return BadRequest("Método de pago no válido");

            var factura = new Factura(
                Guid.NewGuid(),
                usuario.Id,
                metodoPago
            );

            // 3. Agregar detalles a la factura
            foreach (var item in usuario.Carrito)
            {
                var detalle = new DetalleFactura(
                    Guid.NewGuid(),
                    factura.Id,
                    item.TipoItem,
                    item.ItemId,
                    item.Cantidad,
                    item.PrecioUnitario
                );
                
                factura.AgregarDetalle(detalle);
                
                // Si es un artículo, reducir stock
                if (item.TipoItem == TipoItem.Articulo)
                {
                    var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ItemId);
                    if (articulo != null)
                    {
                        if (!articulo.ReducirStock(item.Cantidad))
                            return BadRequest($"No hay suficiente stock para {item.ItemId}");
                    }
                }
            }

            // 4. Guardar factura en la base de datos
            await _unitOfWork.Facturas.AddAsync(factura);
            
            // 5. Iniciar transacción para asegurar consistencia
            await _unitOfWork.BeginTransactionAsync();
            
            // 6. Crear sesión de pago en Stripe
            var successUrl = "http://localhost:5173/checkout/success?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = "http://localhost:5173/checkout/cancel";
            
            var stripeSessionUrl = await _stripeService.CreateCheckoutSession(
                factura, 
                successUrl, 
                cancelUrl
            );
            
            // 7. Vaciar el carrito
            usuario.VaciarCarrito();
            
            // 8. Guardar cambios
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            // 9. Devolver URL de Stripe
            return Ok(new CheckoutResponseDto
            {
                Success = true,
                RedirectUrl = stripeSessionUrl,
                FacturaId = factura.Id
            });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new CheckoutResponseDto
            {
                Success = false,
                ErrorMessage = $"Error al procesar el checkout: {ex.Message}"
            });
        }
    }

    [HttpPost("agregar-item-sql/{articuloId}")]
    [AllowAnonymous]
    public async Task<IActionResult> AgregarItemSQL(Guid articuloId)
    {
        try
        {
            Guid usuarioId = Guid.Parse("65cbfc9f-65f2-4813-b88c-84dc4b7b8757");
            Guid itemId = Guid.NewGuid();
            
            // Verificar que el artículo existe
            var servicio = await _unitOfWork.Servicios.GetByIdAsync(articuloId);
            if (servicio == null)
                return NotFound("El servicio no existe en la base de datos");
            
            // Obtener el primer precio disponible (puedes modificar esto según tu lógica de negocio)
            var precio = servicio.Precios.FirstOrDefault()?.Valor ?? Dinero.Create(0);

            // Obtener el valor numérico del precio
            decimal precioServicio = precio.Cantidad;

            // Crear el item para el carrito
            var itemCarrito = new ItemCarrito(
                Guid.NewGuid(),
                usuarioId,
                TipoItem.Servicio,
                articuloId,
                1,
                Dinero.Create(precioServicio)  // Crear objeto Dinero adecuadamente
            );

            // Ejecutar SQL directamente
            var dbContext = _unitOfWork.GetDbContext();
            var fechaCreacion = DateTime.UtcNow;
            
            var sql = @"
                INSERT INTO [dbo].[ItemsCarrito] (
                    [Id], 
                    [UsuarioId], 
                    [TipoItem], 
                    [ItemId], 
                    [Cantidad], 
                    [PrecioUnitario], 
                    [Moneda], 
                    [CreadoEn], 
                    [ModificadoEn]
                )
                VALUES (
                    @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8
                )";
            
            var parametros = new object[] {
                itemId,                 // Id
                usuarioId,              // UsuarioId
                (int)TipoItem.Servicio, // TipoItem (como entero)
                articuloId,             // ItemId
                1,                      // Cantidad
                precioServicio,         // PrecioUnitario
                servicio.Precios.FirstOrDefault()?.Valor.Moneda ?? "EUR", // Moneda
                fechaCreacion,          // CreadoEn
                fechaCreacion           // ModificadoEn
            };
            
            var filasAfectadas = await dbContext.Database.ExecuteSqlRawAsync(sql, parametros);
            
            if (filasAfectadas > 0)
            {
                return Ok(new { 
                    mensaje = "Artículo agregado al carrito correctamente", 
                    itemId,
                    articuloId,
                    precio = precioServicio,
                    moneda = servicio.Precios.FirstOrDefault()?.Valor.Moneda ?? "EUR",
                    cantidad = 1
                });
            }
            else
            {
                return BadRequest("No se pudo agregar el ítem al carrito");
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                error = ex.Message, 
                detalle = ex.InnerException?.Message,
                stack = ex.StackTrace
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class TestDataController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TestDataController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("crear-articulo")]
    public async Task<IActionResult> CrearArticuloPrueba()
    {
        try
        {
            // Crear un artículo de prueba
            var precioArticulo = Dinero.Create(19.99m);
            var articulo = new Articulo(
                Guid.NewGuid(),
                "Collar para perro",
                "Collar resistente para perros de todas las razas",
                precioArticulo,
                10, // Stock
                CategoriaArticulo.Accesorio
            );

            // Opcional: Agregar imágenes
            var imagen = new ImagenArticulo(
                Guid.NewGuid(),
                articulo.Id,
                "https://via.placeholder.com/300/09f/fff.png", // URL placeholder
                true // Es principal
            );
            articulo.AgregarImagen(imagen);

            // Guardar en la base de datos
            await _unitOfWork.Articulos.AddAsync(articulo);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new {
                mensaje = "Artículo creado correctamente",
                articuloId = articulo.Id,
                nombre = articulo.Nombre,
                precio = articulo.Precio.Cantidad,
                stock = articulo.Stock
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Endpoint para mostrar todos los artículos
    [HttpGet("articulos")]
    public async Task<IActionResult> ObtenerArticulos()
    {
        var articulos = await _unitOfWork.Articulos.GetAllAsync();
        return Ok(articulos.Select(a => new {
            id = a.Id,
            nombre = a.Nombre,
            descripcion = a.Descripcion,
            precio = a.Precio.Cantidad,
            stock = a.Stock,
            categoria = a.Categoria.ToString(),
            imagenes = a.Imagenes.Select(i => new {
                id = i.Id,
                url = i.UrlImagen,
                esPrincipal = i.EsPrincipal
            })
        }));
    }
}
