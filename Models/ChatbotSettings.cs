namespace San_Agustin_Final.Models
{
    public class ChatbotSettings
    {
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public string ApiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    }
}