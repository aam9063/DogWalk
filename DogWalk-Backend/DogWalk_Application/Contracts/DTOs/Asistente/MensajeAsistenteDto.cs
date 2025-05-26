using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Asistente
{
    public class MensajeAsistenteDto
    {
        public string Mensaje { get; set; }
        public string Contexto { get; set; }
        public Dictionary<string, string> MetaDatos { get; set; } = new Dictionary<string, string>();
    }

    public class RespuestaAsistenteDto
    {
        public string Respuesta { get; set; }
        public List<string> SugerenciasAccion { get; set; } = new List<string>();
        public bool RequiereMasContexto { get; set; }
    }
}