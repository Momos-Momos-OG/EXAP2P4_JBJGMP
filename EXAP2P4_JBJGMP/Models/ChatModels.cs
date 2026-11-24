using System.Text.Json.Serialization;

namespace GroqChatbotMvc.Models
{
    public class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; 

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class GroqChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "openai/gpt-oss-20b";

        [JsonPropertyName("reasoning_effort")]
        public string ReasoningEffort { get; set; } = "low";

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.6;

        [JsonPropertyName("max_completion_tokens")]
        public int MaxCompletionTokens { get; set; } = 1500;

        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = new();
    }

    public class GroqChatResponse
    {
        [JsonPropertyName("choices")]
        public List<GroqChoice> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public GroqUsage? Usage { get; set; }
    }

    public class GroqChoice
    {
        [JsonPropertyName("message")]
        public GroqMessage Message { get; set; } = new();
    }

    public class GroqUsage
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class ChatViewModel
    {
        public List<GroqMessage> MensajesVisibles { get; set; } = new();
        public string ModeloActivo { get; set; } = "openai/gpt-oss-20b";
        public int UltimosTokens { get; set; }
        public string? Error { get; set; }
    }
}
