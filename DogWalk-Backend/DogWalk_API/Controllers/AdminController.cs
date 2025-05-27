using DogWalk_Application.Contracts.DTOs.Admin;
using DogWalk_Application.Features.Admin.Commands;
using DogWalk_Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DogWalk_API.Controllers
{
    /// <summary>
    /// Controlador que maneja todas las operaciones relacionadas con el administrador.
    /// Incluye gestión de usuarios, creación de administradores y asignación de roles.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Constructor del controlador de administración.
        /// </summary>
        /// <param name="mediator">Mediador para la comunicación entre componentes</param>
        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crea un nuevo administrador.
        /// </summary>
        /// <param name="command">Comando para crear un administrador</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Si el administrador se creó correctamente</response>
        /// <response code="400">Si el administrador no se creó correctamente</response>
        /// <response code="500">Si ocurre un error al crear el administrador</response>
        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="command">Comando para asignar un rol</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="204">Si el rol se asignó correctamente</response>
        /// <response code="400">Si el rol no se asignó correctamente</response>
        /// <response code="500">Si ocurre un error al asignar el rol</response>
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(AssignRoleCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Obtiene el resumen del dashboard.
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Si el resumen se obtuvo correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="500">Si ocurre un error al obtener el resumen</response>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _mediator.Send(new GetDashboardSummaryQuery());
            return Ok(result);
        }

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200">Si los usuarios se obtuvieron correctamente</response>
        /// <response code="401">Si el usuario no está autenticado</response>
        /// <response code="500">Si ocurre un error al obtener los usuarios</response>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }

        /// <summary>
        /// Elimina un usuario.
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="204">Si el usuario se eliminó correctamente</response>
        /// <response code="400">Si el usuario no se eliminó correctamente</response>
        /// <response code="500">Si ocurre un error al eliminar el usuario</response>
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            try
            {
                var result = await _mediator.Send(new DeleteUserCommand(userId));
                if (!result)
                    return NotFound("Usuario no encontrado");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
