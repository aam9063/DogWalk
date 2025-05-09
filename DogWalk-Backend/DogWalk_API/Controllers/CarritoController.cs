using DogWalk_Application.Contracts.DTOs.Carrito;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DogWalk_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarritoController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailService _emailService;

        public CarritoController(IUnitOfWork unitOfWork, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        // Helper para obtener el ID del usuario autenticado
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

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

        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarArticulo([FromBody] AddItemCarritoDto dto)
        {
            try
            {
                if (dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor que cero");

                await _unitOfWork.BeginTransactionAsync();

                var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                if (usuario == null) 
                    return NotFound("Usuario no encontrado");

                var articulo = await _unitOfWork.Articulos.GetByIdAsync(dto.ArticuloId);
                if (articulo == null)
                    return NotFound("Artículo no encontrado");

                if (articulo.Stock < dto.Cantidad)
                    return BadRequest("No hay suficiente stock disponible");

                try
                {
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

                    return Ok(new { mensaje = "Artículo agregado al carrito correctamente" });
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al agregar al carrito", detalle = ex.Message });
            }
        }

        [HttpPut("actualizar")]
        public async Task<IActionResult> ActualizarCantidad([FromBody] UpdateItemCarritoDto dto)
        {
            try
            {
                if (dto.Cantidad <= 0)
                    return BadRequest("La cantidad debe ser mayor que cero");

                await _unitOfWork.BeginTransactionAsync();

                var usuario = await _unitOfWork.Usuarios.GetByIdWithCarritoAsync(GetUserId());
                if (usuario == null) 
                    return NotFound("Usuario no encontrado");

                var itemCarrito = usuario.Carrito.FirstOrDefault(i => i.Id == dto.ItemCarritoId);
                if (itemCarrito == null)
                    return NotFound("Item no encontrado en el carrito");

                // Verificar stock disponible
                var articulo = itemCarrito.Articulo;
                if (articulo != null && articulo.Stock < dto.Cantidad)
                    return BadRequest("No hay suficiente stock disponible");

                try
                {
                    itemCarrito.ActualizarCantidad(dto.Cantidad);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(new { mensaje = "Cantidad actualizada correctamente" });
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al actualizar cantidad", detalle = ex.Message });
            }
        }

        [HttpDelete("eliminar/{itemCarritoId}")]
        public async Task<IActionResult> EliminarItem(Guid itemCarritoId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
            if (usuario == null) return NotFound("Usuario no encontrado");

            usuario.EliminarItemCarrito(itemCarritoId);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { mensaje = "Artículo eliminado del carrito" });
        }

        [HttpPost("vaciar")]
        public async Task<IActionResult> VaciarCarrito()
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
            if (usuario == null) return NotFound("Usuario no encontrado");

            usuario.VaciarCarrito();
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { mensaje = "Carrito vaciado correctamente" });
        }
    }
}
