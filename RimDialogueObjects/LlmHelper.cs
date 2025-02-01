using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Azure.AI.OpenAI;
using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Config;
using GroqSharp.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimDialogueObjects
{
  public static class LlmHelper
  {
    public static async Task<string> GetResults(IConfiguration configuration, string prompt)
    {
      var provider = configuration["provider"];
      try
      {
        switch (provider?.ToUpper())
        {
          case "AWS":
            var awsKey = configuration["AwsKey"];
            if (String.IsNullOrWhiteSpace(awsKey))
              throw new Exception("AWS Key is empty in appsettings.");
            var awsSecret = configuration["AwsSecret"];
            if (String.IsNullOrWhiteSpace(awsSecret))
              throw new Exception("AWS Secret is empty in appsettings.");
            var awsRegion = configuration["AwsRegion"];
            if (String.IsNullOrWhiteSpace(awsRegion))
              throw new Exception("AWS Region is empty in appsettings.");
            var modelId = configuration["AwsModelId"];
            if (String.IsNullOrWhiteSpace(modelId))
              throw new Exception("AWS ModelId is empty in appsettings.");
            var maxOutputWords = configuration["MaxOutputWords"];
            if (int.TryParse(maxOutputWords, out int maxTokens))
              maxTokens = 25;
            return await GetFromAws(awsKey, awsSecret, awsRegion, modelId, prompt, maxTokens);
          case "OLLAMA":
            return await GetFromOllama(configuration, prompt);
          case "OPENAI":
            return await GetFromOpenAi(configuration, prompt);
          case "GROQ":
            return await GetFromGroq(configuration, prompt);
          case "GEMINI":
            return await GetFromGemini(configuration, prompt);
          case null:
            throw new Exception($"Provider not set.");
          default:
            throw new Exception($"Unknown provider:'{provider}'");
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error fetching results.", ex);
        exception.Data.Add("provider", provider);
        throw exception;
      }
    }

    public static async Task<string> GetFromAws(
      string awsKey,
      string awsSecret,
      string awsRegion,
      string modelId,
      string prompt,
      int maxTokens)
    {
      try
      {
        BasicAWSCredentials awsCredentials = new(awsKey, awsSecret);
        var region = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
        AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(awsCredentials, region);
                var message = new Amazon.BedrockRuntime.Model.Message();
        message.Content = new List<ContentBlock> { new ContentBlock { Text = prompt } };
        message.Role = ConversationRole.User;
        ConverseRequest request = new ConverseRequest
        {
          ModelId = modelId,
          Messages = new List<Amazon.BedrockRuntime.Model.Message> { message },
          InferenceConfig = new InferenceConfiguration {
            MaxTokens = maxTokens
          }
        };
        var converseResponse = await client.ConverseAsync(request);
        return converseResponse.Output.Message.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("awsRegion", awsRegion);
        exception.Data.Add("modelId", modelId);
        throw exception;
      }
    }

    public static async Task<string> GetFromOpenAi(IConfiguration configuration, string prompt)
    {
      var openAiUri = configuration["OpenAiUri"];
      if (String.IsNullOrWhiteSpace(openAiUri))
        throw new Exception("Provider is set to OpenAi but 'OpenAiUri' is empty in appsettings.");
      var openAiApiKey = configuration["OpenAiApiKey"];
      if (String.IsNullOrWhiteSpace(openAiApiKey))
        throw new Exception("Provider is set to OpenAi but 'OpenAiApiKey' is empty in appsettings.");
      var openAiDeployment = configuration["OpenAiDeployment"];
      if (String.IsNullOrWhiteSpace(openAiDeployment))
        throw new Exception("Provider is set to OpenAi but 'OpenAiDeployment' is empty in appsettings.");
      try
      {
        AzureOpenAIClient azureClient = new(
          new Uri(openAiUri),
          new ApiKeyCredential(openAiApiKey));
        ChatClient chatClient = azureClient.GetChatClient(openAiDeployment);
        var results = await chatClient.CompleteChatAsync(new OpenAI.Chat.UserChatMessage(prompt));
        return results.Value.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Azure.", ex);
        exception.Data.Add("openAiUri", openAiUri);
        exception.Data.Add("openAiDeployment", openAiDeployment);
        throw exception;
      }
    }

    public static async Task<string> GetFromGroq(IConfiguration configuration, string prompt)
    {
      var apiKey = configuration["GroqApiKey"];
      if (String.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Provider is set to Groq but 'GroqApiKey' is empty in appsettings.");
      var groqModelId = configuration["GroqModelId"];
      if (String.IsNullOrWhiteSpace(groqModelId))
        throw new Exception("Provider is set to Groq but 'GroqModelId' is empty in appsettings.");
      try
      {
        var groqClient = new GroqClient(apiKey, groqModelId);
        var text = await groqClient.CreateChatCompletionAsync(
          new GroqSharp.Models.Message { Role = MessageRoleType.User, Content = prompt });
        if (text == null)
          throw new Exception("Groq response is null.");
        return text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Groq.", ex);
        exception.Data.Add("groqModelId", groqModelId);
        throw exception;
      }
    }

    public static async Task<string> GetFromOllama(IConfiguration configuration, string prompt)
    {
      var ollamaUrl = configuration["OllamaUrl"];
      if (String.IsNullOrWhiteSpace(ollamaUrl))
        throw new Exception("Provider is set to Ollama but 'OllamaUrl' is empty in appsettings.");
      var ollamaModelId = configuration["OllamaModelId"];
      if (String.IsNullOrWhiteSpace(ollamaModelId))
        throw new Exception("Provider is set to Ollama but 'OllamaModelId' is empty in appsettings.");
      try
      {
        IChatClient client = new OllamaChatClient(new Uri(ollamaUrl), ollamaModelId);
        var result = await client.CompleteAsync(
          [
              new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, prompt)
          ]);
        var text = result.Message.Text;
        if (text == null)
          throw new Exception("Ollama response is null.");
        return text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Ollama.", ex);
        exception.Data.Add("ollamaUrl", ollamaUrl);
        exception.Data.Add("ollamaModelId", ollamaModelId);
        throw exception;
      }
    }

    public static async Task<string> GetFromGemini(IConfiguration configuration, string prompt)
    {
      var geminiApiKey = configuration["GeminiApiKey"];
      if (String.IsNullOrWhiteSpace(geminiApiKey))
        throw new Exception("Provider is set to Gemini but 'GeminiApiKey' is empty in appsettings.");
      var geminiUrl = configuration["GeminiUrl"];
      if (String.IsNullOrWhiteSpace(geminiUrl))
        throw new Exception("Provider is set to Gemini but 'GeminiUrl' is empty in appsettings.");
      try
      {
        var client = new GeminiClient(new GoogleGeminiConfig()
        {
          ApiKey = geminiApiKey,
          TextBaseUrl = geminiUrl,
        });
        var response = await client.TextPrompt(prompt);
        if (response == null)
          throw new Exception("Gemini response is null.");
        var candidate = response.Candidates.FirstOrDefault();
        if (candidate == null)
          throw new Exception("Gemini response has no candidates.");
        var part = candidate.Content.Parts.FirstOrDefault();
        if (part == null)
          throw new Exception("Gemini response has no parts.");
        var text = part.Text;
        if (text == null)
          throw new Exception("Gemini response is null.");
        return text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Gemini.", ex);
        exception.Data.Add("geminiUrl", geminiUrl);
        throw exception;
      }
    }
  }
}
