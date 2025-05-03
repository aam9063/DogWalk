using DogWalk_Application.Contracts.DTOs.Articulos;
using DogWalk_Application.Contracts.DTOs.Busqueda;
using DogWalk_Application.Features.Articulos.Commands;
using DogWalk_Application.Features.Articulos.Queries;
using DogWalk_Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DogWalk_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticuloController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public ArticuloController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        // GET: api/Articulo
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
        
        // GET: api/Articulo/categoria/{categoria}
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
        
        // POST: api/Articulo
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Guid>> CreateArticulo(CreateArticuloDto createArticuloDto)
        {
            var command = new CreateArticuloCommand { ArticuloDto = createArticuloDto };
            var resultado = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetArticulo), new { id = resultado }, resultado);
        }
        
        // PUT: api/Articulo/{id}
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
        
        // PATCH: api/Articulo/{id}/stock
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
