using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DogWalk_Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace DogWalk_Infrastructure.Services.OpenAI
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _apiKey;
        private readonly OpenAISettings _settings;

        public OpenAIService(
            HttpClient httpClient,
            ILogger<OpenAIService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey) || (!_apiKey.StartsWith("sk-") && !_apiKey.StartsWith("sk-proj-")))
            {
                _logger.LogError("API Key de OpenAI no válida o no configurada");
                throw new InvalidOperationException("API Key de OpenAI no válida o no configurada");
            }

            _settings = new OpenAISettings
            {
                ModelId = configuration["OpenAI:ModelId"] ?? "gpt-3.5-turbo",
                MaxTokens = int.Parse(configuration["OpenAI:MaxTokens"] ?? "150"),
                Temperature = double.Parse(configuration["OpenAI:Temperature"] ?? "0.7")
            };

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetAsistenteResponseAsync(string mensaje, string contexto, Dictionary<string, string> metaDatos)
        {
            int maxRetries = 3;
            int currentTry = 0;
            int delayMs = 1000; // 1 segundo inicial

            while (currentTry < maxRetries)
            {
                try
                {
                    if (string.IsNullOrEmpty(_apiKey))
                    {
                        _logger.LogError("API Key de OpenAI no configurada");
                        throw new InvalidOperationException("API Key de OpenAI no configurada");
                    }

                    _logger.LogInformation($"Enviando solicitud a OpenAI - Mensaje: {mensaje}, Contexto: {contexto}");

                    var request = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]
                        {
                            new 
                            { 
                                role = "system", 
                                content = "Eres un asistente virtual especializado en DogWalk, una aplicación de paseo de perros."
                            },
                            new { role = "user", content = ConstruirPrompt(mensaje, contexto, metaDatos) }
                        }
                    };

                    _logger.LogInformation($"Request a OpenAI: {JsonSerializer.Serialize(request)}");

                    var response = await _httpClient.PostAsJsonAsync("chat/completions", request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        currentTry++;
                        if (currentTry < maxRetries)
                        {
                            _logger.LogWarning($"Rate limit alcanzado, intento {currentTry} de {maxRetries}. Esperando {delayMs}ms");
                            await Task.Delay(delayMs);
                            delayMs *= 2; // Duplicar el tiempo de espera para el siguiente intento
                            continue;
                        }
                        throw new Exception("Límite de peticiones alcanzado. Por favor, intenta más tarde.");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Error de OpenAI: Status {response.StatusCode}, Content: {responseContent}");
                        throw new Exception($"Error de OpenAI: {response.StatusCode}");
                    }

                    _logger.LogInformation($"Respuesta de OpenAI: {responseContent}");

                    var jsonResponse = JsonDocument.Parse(responseContent);
                    return jsonResponse.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();
                }
                catch (Exception ex) when (currentTry < maxRetries - 1)
                {
                    _logger.LogWarning($"Intento {currentTry + 1} fallido: {ex.Message}");
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                    currentTry++;
                }
            }

            throw new Exception("No se pudo completar la solicitud después de varios intentos");
        }

        private string ConstruirPrompt(string mensaje, string contexto, Dictionary<string, string> metaDatos)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Contexto actual: {contexto}");
            
            if (metaDatos.Any())
            {
                promptBuilder.AppendLine("\nInformación adicional:");
                foreach (var meta in metaDatos)
                {
                    promptBuilder.AppendLine($"- {meta.Key}: {meta.Value}");
                }
            }

            promptBuilder.AppendLine($"\nPregunta del usuario: {mensaje}");
            return promptBuilder.ToString();
        }
    }
}