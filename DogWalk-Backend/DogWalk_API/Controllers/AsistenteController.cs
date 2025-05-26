using DogWalk_Application.Contracts.DTOs.Asistente;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Services.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace DogWalk_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenteController : ControllerBase
    {
        private readonly OpenAIService _openAIService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AsistenteController> _logger;

        public AsistenteController(
            OpenAIService openAIService,
            IUnitOfWork unitOfWork,
            ILogger<AsistenteController> logger)
        {
            _openAIService = openAIService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost("consultar")]
        [AllowAnonymous] // O usa [Authorize] si quieres que requiera autenticación
        public async Task<ActionResult<RespuestaAsistenteDto>> ConsultarAsistente(
            [FromBody] MensajeAsistenteDto mensaje)
        {
            try
            {
                var respuesta = await _openAIService.GetAsistenteResponseAsync(
                    mensaje.Mensaje,
                    mensaje.Contexto,
                    mensaje.MetaDatos
                );

                return Ok(new RespuestaAsistenteDto
                {
                    Respuesta = respuesta,
                    SugerenciasAccion = new List<string>(),
                    RequiereMasContexto = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la consulta al asistente");
                
                var mensajeError = ex.Message.Contains("Límite de peticiones")
                    ? "El servicio está experimentando alta demanda. Por favor, intenta de nuevo en unos momentos."
                    : "Hubo un error al procesar tu consulta. Por favor, intenta de nuevo.";

                return StatusCode(429, new { 
                    error = mensajeError,
                    detalles = ex.Message,
                    sugerencia = "Intenta de nuevo en unos segundos"
                });
            }
        }

        private async Task<Dictionary<string, string>> ObtenerContextoAplicacion(string contexto)
        {
            var infoContexto = new Dictionary<string, string>();

            // Añadir información según el contexto
            switch (contexto.ToLower())
            {
                case "paseadores":
                    var paseadoresActivos = await _unitOfWork.Paseadores.GetByDisponibilidadAsync(true);
                    infoContexto["paseadores_disponibles"] = paseadoresActivos.Count().ToString();
                    infoContexto["servicios_disponibles"] = (await _unitOfWork.Servicios.GetAllAsync()).Count().ToString();
                    break;

                case "reservas":
                    if (User.Identity.IsAuthenticated)
                    {
                        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                        var reservasActivas = await _unitOfWork.Reservas.GetByUsuarioIdAsync(userId);
                        infoContexto["reservas_activas"] = reservasActivas.Count().ToString();
                    }
                    break;

                case "tienda":
                    var articulosDisponibles = await _unitOfWork.Articulos.GetByDisponibilidadAsync(true);
                    infoContexto["articulos_disponibles"] = articulosDisponibles.Count().ToString();
                    break;
            }

            return infoContexto;
        }

        private async Task<List<string>> GenerarSugerencias(string contexto)
        {
            var sugerencias = new List<string>();

            switch (contexto.ToLower())
            {
                case "paseadores":
                    sugerencias.Add("/paseadores/buscar");
                    sugerencias.Add("/paseadores/reservar");
                    break;
                case "tienda":
                    sugerencias.Add("/tienda/productos");
                    sugerencias.Add("/tienda/carrito");
                    break;
            }

            return await Task.FromResult(sugerencias);
        }
    }
}