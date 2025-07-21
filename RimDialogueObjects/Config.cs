namespace RimDialogueObjects
{
  public class Config
  {
    public string? Name { get; set; }
    public float RateLimit { get; set; }
    public int RateLimitCacheMinutes { get; set; }
    public int MinRateLimitRequestCount { get; set; }
    public int MaxPromptLength { get; set; }
    public int MaxResponseLength { get; set; }
    public int MaxOutputWords { get; set; }
    public bool RemoveThinking { get; set; }
    public required Model[] Models { get; set; }
  }

  public class Model
  {
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public string? AwsModelId { get; set; }
    public string? AwsRegion { get; set; }
    public string? AwsKey { get; set; }
    public string? AwsSecret { get; set; }
    public string? OpenAiApiKey { get; set; }
    public string? OpenAiModel { get; set; }
    public string? OpenAiResourceName { get; set; }
    public string? OpenAiVersion { get; set; }
    public string? GeminiApiKey { get; set; }
    public string? GeminiModel { get; set; }
    public string? OllamaUrl { get; set; }
    public string? OllamaModelId { get; set; }
    public string? GroqApiKey { get; set; }
    public string? GroqModelId { get; set; }
  }
}
