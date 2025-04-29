using DogWalk_Application.Contracts.DTOs.Carrito;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
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

        public CarritoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Helper para obtener el ID del usuario autenticado
        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpGet]
        public async Task<ActionResult<CarritoDto>> GetCarrito()
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
            if (usuario == null) return NotFound("Usuario no encontrado");

            var carritoItems = new List<ItemCarritoDto>();

            foreach (var item in usuario.Carrito)
            {
                string nombreItem = "";
                string imagenUrl = "";

                // Obtener nombre e imagen según el tipo de ítem
                if (item.TipoItem == TipoItem.Articulo)
                {
                    var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ItemId);
                    if (articulo != null)
                    {
                        nombreItem = articulo.Nombre;
                        // Obtener imagen principal si existe
                        var imagenPrincipal = articulo.Imagenes.FirstOrDefault(i => i.EsPrincipal);
                        if (imagenPrincipal != null)
                            imagenUrl = imagenPrincipal.UrlImagen;
                    }
                }
                else if (item.TipoItem == TipoItem.Servicio)
                {
                    var servicio = await _unitOfWork.Servicios.GetByIdAsync(item.ItemId);
                    if (servicio != null)
                    {
                        nombreItem = servicio.Nombre;
                        imagenUrl = ""; // Servicio no tiene imagen directa
                    }
                }

                carritoItems.Add(new ItemCarritoDto
                {
                    Id = item.Id,
                    TipoItem = item.TipoItem.ToString(),
                    ItemId = item.ItemId,
                    NombreItem = nombreItem,
                    ImagenUrl = imagenUrl,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario.Cantidad,
                    Subtotal = item.Subtotal.Cantidad
                });
            }

            var totalCarrito = carritoItems.Sum(x => x.Subtotal);
            var cantidadItems = carritoItems.Sum(x => x.Cantidad);

            return Ok(new CarritoDto
            {
                UsuarioId = usuario.Id,
                Items = carritoItems,
                Total = totalCarrito,
                CantidadItems = cantidadItems
            });
        }

        [HttpPost("agregar")]
        public async Task<IActionResult> AgregarItem([FromBody] AddItemCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero");

            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
            if (usuario == null) return NotFound("Usuario no encontrado");

            // Convertir string a enum
            TipoItem tipoItem;
            if (!Enum.TryParse(dto.TipoItem, true, out tipoItem))
                return BadRequest("Tipo de ítem no válido");

            // Obtener precio según el tipo de ítem
            Dinero precioUnitario;
            
            if (tipoItem == TipoItem.Articulo)
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(dto.ItemId);
                if (articulo == null)
                    return NotFound("Artículo no encontrado");
                    
                // Verificar stock disponible
                if (articulo.Stock < dto.Cantidad)
                    return BadRequest("No hay suficiente stock disponible");
                    
                precioUnitario = articulo.Precio;
            }
            else // Servicio
            {
                var servicio = await _unitOfWork.Servicios.GetByIdAsync(dto.ItemId);
                if (servicio == null)
                    return NotFound("Servicio no encontrado");
                    
                var precios = servicio.Precios.ToList();
                if (!precios.Any())
                    return BadRequest("El servicio no tiene precios definidos");
                
                // Opción 1: Usar el precio más bajo
                var precio = precios.OrderBy(p => p.Valor.Cantidad).First();
                
                // Alternativa: Opción 2 - Si quieres usar el precio de un paseador específico:
                // var paseadorId = Guid.Parse("ID-del-paseador"); // Obtener de algún lado
                // var precio = precios.FirstOrDefault(p => p.PaseadorId == paseadorId);
                // if (precio == null)
                //     return BadRequest("No hay precio definido para este paseador");
                
                precioUnitario = precio.Valor;
            }

            // Crear ítem del carrito
            var item = new ItemCarrito(
                Guid.NewGuid(),
                usuario.Id,
                tipoItem,
                dto.ItemId,
                dto.Cantidad,
                precioUnitario
            );

            // Agregar al carrito del usuario
            usuario.AgregarItemCarrito(item);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { mensaje = "Ítem agregado al carrito correctamente" });
        }

        [HttpPut("actualizar")]
        public async Task<IActionResult> ActualizarCantidad([FromBody] UpdateItemCarritoDto dto)
        {
            if (dto.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero");

            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
            if (usuario == null) return NotFound("Usuario no encontrado");

            var item = usuario.ObtenerItemCarrito(dto.ItemCarritoId);
            if (item == null)
                return NotFound("Ítem no encontrado en el carrito");

            // Si es un artículo, verificar stock
            if (item.TipoItem == TipoItem.Articulo)
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(item.ItemId);
                if (articulo != null && articulo.Stock < dto.Cantidad)
                    return BadRequest("No hay suficiente stock disponible");
            }

            try
            {
                usuario.ActualizarCantidadItemCarrito(dto.ItemCarritoId, dto.Cantidad);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { mensaje = "Cantidad actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("eliminar/{itemCarritoId}")]
        public async Task<IActionResult> EliminarItem(Guid itemCarritoId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(GetUserId());
            if (usuario == null) return NotFound("Usuario no encontrado");

            usuario.EliminarItemCarrito(itemCarritoId);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { mensaje = "Ítem eliminado del carrito" });
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
