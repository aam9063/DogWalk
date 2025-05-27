using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Asistente
{   
    /// <summary>
    /// DTO para representar un mensaje del asistente.
    /// </summary>
    public class MensajeAsistenteDto
    {
        public string Mensaje { get; set; }
        public string Contexto { get; set; }
        public Dictionary<string, string> MetaDatos { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// DTO para representar la respuesta del asistente.
    /// </summary>
    public class RespuestaAsistenteDto
    {
        public string Respuesta { get; set; }
        public List<string> SugerenciasAccion { get; set; } = new List<string>();
        public bool RequiereMasContexto { get; set; }
    }
}