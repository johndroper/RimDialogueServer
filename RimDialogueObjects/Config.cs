namespace RimDialogueObjects
{
  public class Config
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
    public string? GeminiUrl { get; set; }
    public string? OllamaUrl { get; set; }
    public string? OllamaModelId { get; set; }
    public string? GroqApiKey { get; set; }
    public string? GroqModelId { get; set; }
    public float RateLimit { get; set; }
    public int RateLimitCacheMinutes { get; set; }
    public int MinRateLimitRequestCount { get; set; }
    public int MaxPromptLength { get; set; }
    public int MaxResponseLength { get; set; }
    public int MaxOutputWords { get; set; }
    public bool ShowScenario { get; set; }
    public bool ShowSpecialInstructions { get; set; }
    public bool ShowPawnType { get; set; }
    public bool ShowPersonality { get; set; }
    public bool ShowAgeRaceAndGender { get; set; }
    public bool ShowFullName { get; set; }
    public bool ShowDescription { get; set; }
    public bool ShowAnimal { get; set; }
    public bool ShowRoyaltyTitle { get; set; }
    public bool ShowHair { get; set; }
    public bool ShowBeard { get; set; }
    public bool ShowTattoo { get; set; }
    public bool ShowFaction { get; set; }
    public bool ShowIdeology { get; set; }
    public bool ShowPrecepts { get; set; }
    public bool ShowAdulthood { get; set; }
    public bool ShowChildhood { get; set; }
    public bool ShowJob { get; set; }
    public bool ShowCombatLog { get; set; }
    public bool ShowRelations { get; set; }
    public bool ShowTraits { get; set; }
    public bool ShowSkills { get; set; }
    public bool ShowMoodThoughts { get; set; }
    public bool ShowHealth { get; set; }
    public bool ShowApparel { get; set; }
    public bool ShowWeapons { get; set; }
    public bool ShowMoodString { get; set; }
    public bool ShowNeeds { get; set; }
    public bool ShowOpinions { get; set; }
    public bool ShowTimeData { get; set; }
    public bool ShowColonyData { get; set; }
    public bool ShowOtherFactions { get; set; }
    public bool ShowBiome { get; set; }
    public bool ShowWeather { get; set; }
    public bool ShowRecentIncidents { get; set; }
    public bool ShowRoom { get; set; }
    public bool RepeatInstructions { get; set; }
    public bool RemoveThinking { get; set; }
  }
}
