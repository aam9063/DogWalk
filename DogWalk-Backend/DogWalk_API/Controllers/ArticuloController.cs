using DogWalk_Application.Contracts.DTOs.Articulos;
using DogWalk_Application.Contracts.DTOs.Busqueda;
using DogWalk_Application.Features.Articulos.Commands;
using DogWalk_Application.Features.Articulos.Queries;
using DogWalk_Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador para gestionar los artículos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ArticuloController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Constructor del controlador de artículos.
        /// </summary>
        /// <param name="mediator">Mediador para la comunicación entre componentes</param>
        public ArticuloController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Obtiene todos los artículos.
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Si los artículos se obtuvieron correctamente</response>
        /// <response code="500">Si ocurre un error al obtener los artículos</response>
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginadoDto<ArticuloDto>>> GetArticulos(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            [FromQuery] string sortBy = null,
            [FromQuery] bool ascending = true,
            [FromQuery] CategoriaArticulo? categoria = null)
        {
            var query = new GetAllArticulosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                Ascending = ascending,
                Categoria = categoria
            };
            
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }
        
        // GET: api/Articulo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticuloDetailsDto>> GetArticulo(Guid id)
        {
            var query = new GetArticuloByIdQuery { Id = id };
            var resultado = await _mediator.Send(query);
            
            if (resultado == null)
                return NotFound();
                
            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene todos los artículos por categoría.
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Si los artículos se obtuvieron correctamente</response>
        /// <response code="500">Si ocurre un error al obtener los artículos</response>
        
        [HttpGet("categoria/{categoria}")]
        public async Task<ActionResult<ResultadoPaginadoDto<ArticuloDto>>> GetArticulosPorCategoria(
            CategoriaArticulo categoria,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = null,
            [FromQuery] bool ascending = true)
        {
            var query = new GetAllArticulosQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Ascending = ascending,
                Categoria = categoria
            };
            
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }
        
        /// <summary>
        /// Crea un nuevo artículo.
        /// </summary>
        /// <param name="createArticuloDto">Datos del artículo a crear</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="201">Si el artículo se creó correctamente</response>
        /// <response code="400">Si el artículo no se creó correctamente</response>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Guid>> CreateArticulo(CreateArticuloDto createArticuloDto)
        {
            var command = new CreateArticuloCommand { ArticuloDto = createArticuloDto };
            var resultado = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetArticulo), new { id = resultado }, resultado);
        }
        
        /// <summary>
        /// Crea un nuevo artículo.
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        /// <response code="201">Si el artículo se creó correctamente</response>
        /// <response code="400">Si el artículo no se creó correctamente</response>
        [HttpPost("batch")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Guid>>> CreateArticulosBatch(List<CreateArticuloDto> createArticuloDtos)
        {
            var resultados = new List<Guid>();
            
            foreach (var dto in createArticuloDtos)
            {
                var command = new CreateArticuloCommand { ArticuloDto = dto };
                var resultado = await _mediator.Send(command);
                resultados.Add(resultado);
            }
            
            return Ok(resultados);
        }
        
        /// <summary>
        /// Actualiza un artículo existente.
        /// </summary>
        /// <param name="id">ID del artículo a actualizar</param>
        /// <param name="updateArticuloDto">Datos del artículo a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="204">Si el artículo se actualizó correctamente</response>

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateArticulo(Guid id, UpdateArticuloDto updateArticuloDto)
        {
            var command = new UpdateArticuloCommand 
            { 
                Id = id, 
                ArticuloDto = updateArticuloDto 
            };
            
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }
        
        // DELETE: api/Articulo/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteArticulo(Guid id)
        {
            var command = new DeleteArticuloCommand { Id = id };
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }
        
        /// <summary>
        /// Actualiza el stock de un artículo.
        /// </summary>
        /// <param name="id">ID del artículo a actualizar</param>
        /// <param name="cantidad">Cantidad de stock a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="204">Si el stock se actualizó correctamente</response>
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int cantidad)
        {
            var command = new UpdateArticuloStockCommand 
            { 
                Id = id, 
                Cantidad = cantidad 
            };
            
            var resultado = await _mediator.Send(command);
            
            if (!resultado)
                return NotFound();
                
            return NoContent();
        }
    }
}
