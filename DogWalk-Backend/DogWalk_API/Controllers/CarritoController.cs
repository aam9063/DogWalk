using DogWalk_Application.Contracts.DTOs.Carrito;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con el carrito de compras.
    /// Incluye gestión de artículos, cantidad y total del carrito.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarritoController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailService _emailService;

        /// <summary>
        /// Constructor del controlador de carrito.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo para acceso a datos</param>
        /// <param name="emailService">Servicio de envío de correos electrónicos</param>
        public CarritoController(IUnitOfWork unitOfWork, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        // Helper para obtener el ID del usuario autenticado
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


        

        /// <summary>
        /// Obtiene el carrito de compras del usuario autenticado.
        /// </summary>
        /// <returns>Detalles del carrito de compras</returns>
        /// <response code="200">Retorna el carrito de compras</response>
        /// <response code="404">Si el usuario no se encuentra</response>
        [HttpGet]
        public async Task<ActionResult<CarritoDto>> GetCarrito()
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                if (usuario == null)
                    return NotFound("Usuario no encontrado");

                var carritoItems = new List<ItemCarritoDto>();

                foreach (var item in usuario.Carrito)
                {
                    var articulo = item.Articulo; // Ya debería estar cargado por el Include
                    if (articulo == null) continue; // Skip si por alguna razón no existe el artículo

                    var imagenUrl = articulo.Imagenes
                        .FirstOrDefault(i => i.EsPrincipal)?.UrlImagen ?? "";

                    carritoItems.Add(new ItemCarritoDto
                    {
                        Id = item.Id,
                        ArticuloId = item.ArticuloId,
                        NombreArticulo = articulo.Nombre,
                        ImagenUrl = imagenUrl,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario.Cantidad,
                        Subtotal = item.Subtotal.Cantidad
                    });
                }

                return Ok(new CarritoDto
                {
                    UsuarioId = usuario.Id,
                    Items = carritoItems,
                    Total = carritoItems.Sum(x => x.Subtotal),
                    CantidadItems = carritoItems.Sum(x => x.Cantidad)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener el carrito", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Agrega un artículo al carrito de compras del usuario autenticado.
        /// </summary>
        /// <param name="dto">Datos del artículo a agregar</param>
        /// <returns>Confirmación del agregado</returns>
        /// <response code="200">Si el artículo se agregó correctamente</response>
        /// <response code="400">Si la cantidad es inválida</response>
        /// <response code="404">Si el usuario o artículo no se encuentra</response>
        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarArticulo([FromBody] AddItemCarritoDto dto)
        {
            try
            {
                if (dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor que cero");

                var strategy = _unitOfWork.CreateExecutionStrategy();
                
                var result = await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                    if (usuario == null)
                        throw new Exception("Usuario no encontrado");

                    var articulo = await _unitOfWork.Articulos.GetByIdAsync(dto.ArticuloId);
                    if (articulo == null)
                        throw new Exception("Artículo no encontrado");

                    if (articulo.Stock < dto.Cantidad)
                        throw new Exception("No hay suficiente stock disponible");

                    var itemExistente = usuario.Carrito
                        .FirstOrDefault(i => i.ArticuloId == dto.ArticuloId);

                    if (itemExistente != null)
                    {
                        itemExistente.ActualizarCantidad(itemExistente.Cantidad + dto.Cantidad);
                    }
                    else
                    {
                        var precioUnitario = Dinero.Create(articulo.Precio.Cantidad, articulo.Precio.Moneda);
                        var nuevoItem = new ItemCarrito(
                            Guid.NewGuid(),
                            usuario.Id,
                            dto.ArticuloId,
                            dto.Cantidad,
                            precioUnitario
                        );

                        await _unitOfWork.AddAsync(nuevoItem);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return true;
                });

                return Ok(new { mensaje = "Artículo agregado al carrito correctamente" });
            }
            catch (Exception ex)
            {
                // Si la excepción contiene un mensaje específico de validación, lo devolvemos como BadRequest
                if (ex.Message is "Usuario no encontrado" or "Artículo no encontrado")
                    return NotFound(ex.Message);
                if (ex.Message == "No hay suficiente stock disponible")
                    return BadRequest(ex.Message);

                return StatusCode(500, new { error = "Error al agregar al carrito", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza la cantidad de un artículo en el carrito de compras del usuario autenticado.
        /// </summary>
        /// <param name="dto">Datos de actualización de cantidad</param>
        /// <returns>Confirmación de la actualización</returns>
        /// <response code="200">Si la cantidad se actualizó correctamente</response>
        /// <response code="400">Si la cantidad es inválida</response>
        /// <response code="404">Si el usuario o artículo no se encuentra</response>
        [HttpPut("actualizar")]
        public async Task<IActionResult> ActualizarCantidad([FromBody] UpdateItemCarritoDto dto)
        {
            try
            {
                if (dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor que cero");

                var strategy = _unitOfWork.CreateExecutionStrategy();
                
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                    if (usuario == null)
                        throw new Exception("Usuario no encontrado");

                    var itemCarrito = usuario.Carrito.FirstOrDefault(i => i.Id == dto.ItemCarritoId);
                    if (itemCarrito == null)
                        throw new Exception("Item no encontrado en el carrito");

                    // Verificar stock disponible
                    var articulo = itemCarrito.Articulo;
                    if (articulo != null && articulo.Stock < dto.Cantidad)
                        throw new Exception("No hay suficiente stock disponible");

                    itemCarrito.ActualizarCantidad(dto.Cantidad);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return true;
                });

                return Ok(new { mensaje = "Cantidad actualizada correctamente" });
            }
            catch (Exception ex)
            {
                if (ex.Message is "Usuario no encontrado" or "Item no encontrado en el carrito")
                    return NotFound(ex.Message);
                if (ex.Message == "No hay suficiente stock disponible")
                    return BadRequest(ex.Message);

                return StatusCode(500, new { error = "Error al actualizar cantidad", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un artículo del carrito de compras del usuario autenticado.
        /// </summary>
        /// <param name="itemCarritoId">ID del artículo a eliminar</param>
        /// <returns>Confirmación de la eliminación</returns>
        /// <response code="200">Si el artículo se eliminó correctamente</response>
        /// <response code="404">Si el usuario o artículo no se encuentra</response>
        /// <response code="500">Si ocurre un error al eliminar el artículo</response>
        [HttpDelete("eliminar/{itemCarritoId}")]
        public async Task<IActionResult> EliminarItem(Guid itemCarritoId)
        {
            try
            {
                var strategy = _unitOfWork.CreateExecutionStrategy();
                
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                    if (usuario == null) 
                        throw new Exception("Usuario no encontrado");

                    usuario.EliminarItemCarrito(itemCarritoId);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return true;
                });

                return Ok(new { mensaje = "Artículo eliminado del carrito" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Usuario no encontrado")
                    return NotFound(ex.Message);

                return StatusCode(500, new { error = "Error al eliminar el artículo", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Vacia el carrito de compras del usuario autenticado.
        /// </summary>
        /// <returns>Confirmación de la vaciación</returns>
        /// <response code="200">Si el carrito se vació correctamente</response>
        /// <response code="404">Si el usuario no se encuentra</response>
        /// <response code="500">Si ocurre un error al vaciar el carrito</response>
        [HttpPost("vaciar")]
        public async Task<IActionResult> VaciarCarrito()
        {
            try
            {
                var strategy = _unitOfWork.CreateExecutionStrategy();
                
                await strategy.ExecuteAsync(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                    if (usuario == null) 
                        throw new Exception("Usuario no encontrado");

                    usuario.VaciarCarrito();
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return true;
                });

                return Ok(new { mensaje = "Carrito vaciado correctamente" });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Usuario no encontrado")
                    return NotFound(ex.Message);

                return StatusCode(500, new { error = "Error al vaciar el carrito", detalle = ex.Message });
            }
        }
    }
}
