namespace DogWalk_Infrastructure.Configuration
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; }
        public string ModelId { get; set; }
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
    }
}