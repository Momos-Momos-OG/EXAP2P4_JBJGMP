using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GroqChatbotMvc.Models;

namespace GroqChatbotMvc.Services
{
    public class GroqChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
        private const string Modelo = "openai/gpt-oss-20b";
        private const string ApiKeyPlaceholder = "TU_API_KEY_DE_GROQ_AQUI";

        private readonly GroqMessage _systemPrompt = new()
        {
            Role = "system",
            Content = "Eres un asistente de inteligencia artificial claro, útil y directo. Responde siempre en español."
        };

        private readonly List<GroqMessage> _historial = new();
        private int _ultimosTokens = 0;

        public GroqChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public ChatViewModel ObtenerEstadoActual(string? error = null)
        {
            return new ChatViewModel
            {
                MensajesVisibles = _historial.ToList(),
                ModeloActivo = Modelo,
                UltimosTokens = _ultimosTokens,
                Error = error
            };
        }

        public void LimpiarConversacion()
        {
            _historial.Clear();
            _ultimosTokens = 0;
        }

        public async Task<ChatViewModel> EnviarMensajeAsync(string textoUsuario)
        {
            if (string.IsNullOrWhiteSpace(textoUsuario))
                return ObtenerEstadoActual("Escribe un mensaje.");

            string apiKey = _configuration["GroqApi:ApiKey"]?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == ApiKeyPlaceholder)
                return ObtenerEstadoActual("No existe API KEY");

            _historial.Add(new GroqMessage { Role = "user", Content = textoUsuario.Trim() });

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var mensajesAEnviar = new List<GroqMessage> { _systemPrompt };
                mensajesAEnviar.AddRange(_historial.TakeLast(6));

                var requestPayload = new GroqChatRequest
                {
                    Model = Modelo,
                    ReasoningEffort = "low",
                    Temperature = 0.6,
                    MaxCompletionTokens = 1500,
                    Messages = mensajesAEnviar
                };

                string jsonBody = JsonSerializer.Serialize(requestPayload);
                using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(Endpoint, content);
                string responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _historial.RemoveAt(_historial.Count - 1);
                    return ObtenerEstadoActual($"Error HTTP {(int)response.StatusCode}.");
                }

                var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseJson);
                string? contenido = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    _historial.RemoveAt(_historial.Count - 1);
                    return ObtenerEstadoActual("Respuesta vacía.");
                }

                _historial.Add(new GroqMessage { Role = "assistant", Content = contenido.Trim() });
                _ultimosTokens = groqResponse?.Usage?.TotalTokens ?? 0;
                return ObtenerEstadoActual();
            }
            catch (Exception ex)
            {
                _historial.RemoveAt(_historial.Count - 1);
                return ObtenerEstadoActual($"Error de conexión: {ex.Message}");
            }
        }
    }
}
