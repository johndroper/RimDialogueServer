using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using GroqSharp.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Mscc.GenerativeAI;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using RimDialogue.Core;
using RimDialogueObjects.Templates;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RimDialogueObjects
{
  public static class LlmHelper
  {

    public static DialogueResponse SerializeResponse(
      string text,
      IConfiguration configuration,
      float rate,
      out bool outputTruncated)
    {
      DialogueResponse? dialogueResponse = null;
      try
      {
        dialogueResponse = new DialogueResponse();
        int maxResponseLength = configuration.GetValue<int>("MaxResponseLength", 5000);
        if (text.Length > maxResponseLength)
        {
          outputTruncated = true;
          text = text.Substring(0, maxResponseLength) + "...";
        }
        else
          outputTruncated = false;
        dialogueResponse.Text = text;
        dialogueResponse.Rate = rate;
        return dialogueResponse;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error creating DialogueResponse.", ex);
        exception.Data.Add("text", text);
        throw exception;
      }
    }

    public async static Task<LlmResults> GenerateResponse(string prompt, Config config, string modelName)
    {
      try
      {
        if (config.RemoveThinking)
          prompt += "\r\n/no_think";
        var results = await GetResults(config, prompt, modelName);
        if (config.RemoveThinking && results.Text != null)
          results.Text = Regex.Replace(results.Text, "<think>(.|\n)*?</think>", "").Trim();
        return results;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred fetching results.", ex);
        exception.Data.Add("prompt", prompt);
        throw exception;
      }
    }

    //One Pawn
    public static string Generate<DataT, TemplateT>(
      Config config,
      PawnData initiator,
      DataT dialogueData,
      out bool wasTruncated,
      out string prompt) where DataT : RimDialogue.Core.InteractionData.DialogueData where TemplateT : IPromptTemplate<DataT>, new()
    {
      //******Prompt Generation******
      try
      {
        wasTruncated = false;
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        TemplateT promptTemplate = new();
        promptTemplate.Initiator = initiator;
        promptTemplate.Data = dialogueData;
        promptTemplate.Config = config;
        prompt = promptTemplate.TransformText().Trim();
        if (prompt == null)
          throw new Exception("Prompt is null.");
        int maxPromptLength = config.MaxPromptLength;
        if (prompt.Length > maxPromptLength)
        {
          wasTruncated = true;
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          return prompt.Substring(0, maxPromptLength);
        }
        return prompt;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }
    }

    //One pawn with target
    public static string Generate<DataT, TemplateT>(
      Config config,
      PawnData initiator,
      DataT dialogueData,
      PawnData? targetData,
      out bool wasTruncated) where DataT : RimDialogue.Core.InteractionData.DialogueTargetData where TemplateT : ITargetPromptTemplate<DataT>, new()
    {
      //******Prompt Generation******
      string? prompt = null;
      try
      {
        wasTruncated = false;
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        TemplateT promptTemplate = new();
        promptTemplate.Initiator = initiator;
        promptTemplate.Data = dialogueData;
        promptTemplate.Target = targetData;
        promptTemplate.Config = config;
        prompt = promptTemplate.TransformText().Trim();
        if (prompt == null)
          throw new Exception("Prompt is null.");
        int maxPromptLength = config.MaxPromptLength;
        if (prompt.Length > maxPromptLength)
        {
          wasTruncated = true;
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          return prompt.Substring(0, maxPromptLength);
        }
        return prompt;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }
    }


    //Two pawn
    public static string Generate<DataT, TemplateT>(
      Config config,
      PawnData initiator,
      PawnData recipient,
      DataT dialogueData,
      out bool wasTruncated,
      out string prompt) where DataT : RimDialogue.Core.InteractionData.DialogueData where TemplateT : IRecipientPromptTemplate<DataT>, new()
    {
      //******Prompt Generation******
      try
      {
        wasTruncated = false;
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        TemplateT promptTemplate = new();
        promptTemplate.Initiator = initiator;
        promptTemplate.Recipient = recipient;
        promptTemplate.Data = dialogueData;
        promptTemplate.Config = config;
        prompt = promptTemplate.TransformText().Trim();
        if (prompt == null)
          throw new Exception("Prompt is null.");
        int maxPromptLength = config.MaxPromptLength;
        if (prompt.Length > maxPromptLength)
        {
          wasTruncated = true;
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          return prompt.Substring(0, maxPromptLength);
        }
        return prompt;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }
    }

    //Two pawn with target 
    public static string Generate<DataT, TemplateT>(
      Config config,
      PawnData initiator,
      PawnData recipient,
      DataT dialogueData,
      PawnData? targetData,
    out bool wasTruncated) where DataT : RimDialogue.Core.InteractionData.DialogueTargetData where TemplateT : IRecipientTargetPromptTemplate<DataT>, new()
    {
      //******Prompt Generation******
      string? prompt = null;
      try
      {
        wasTruncated = false;
        if (dialogueData == null)
          throw new Exception("dialogueData is null.");
        TemplateT promptTemplate = new();
        promptTemplate.Initiator = initiator;
        promptTemplate.Recipient = recipient;
        promptTemplate.Data = dialogueData;
        promptTemplate.Target = targetData;
        promptTemplate.Config = config;
        prompt = promptTemplate.TransformText().Trim();
        if (prompt == null)
          throw new Exception("Prompt is null.");
        int maxPromptLength = config.MaxPromptLength;
        if (prompt.Length > maxPromptLength)
        {
          wasTruncated = true;
          Console.WriteLine($"Prompt truncated to {maxPromptLength} characters. Original length was {prompt.Length} characters.");
          return prompt.Substring(0, maxPromptLength);
        }
        return prompt;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred generating prompt.", ex);
        exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
        throw exception;
      }
    }

    public class LlmResults
    {
      public LlmResults(string text, string model, long responseTime)
      {
        Text = text;
        Model = model;
        ResponseTime = responseTime;
      }

      public string? Text { get; set; }
      public string Model { get; set; }
      public long ResponseTime { get; set; }
    }


    public static async Task<LlmResults> GetResults(Config config, string prompt, string modelName)
    {

      var model = config.Models.FirstOrDefault(m => m.Name == modelName);
      if (model == null)
        model = config.Models[0];

      try
      {
        switch (model.Provider)
        {
          case "AWS":
            return await GetFromAws(model, prompt);
          case "OLLAMA":
            return await GetFromOllama(model, prompt);
          case "OPENAI":
            return await GetFromOpenAI(model, prompt);
          case "GROQ":
            return await GetFromGroq(model, prompt);
          case "GOOGLE":
            return await GetFromGemini(model, prompt);
          case null:
            throw new Exception($"Provider not set.");
          default:
            throw new Exception($"Unknown provider:'{model.Provider}'");
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error fetching results.", ex);
        exception.Data.Add("provider", model.Provider);
        throw exception;
      }
    }

    public static async Task<LlmResults> GetFromAws(
      Model model,
      string prompt)
    {
      try
      {
        BasicAWSCredentials awsCredentials = new(model.AwsKey, model.AwsSecret);
        var region = Amazon.RegionEndpoint.GetBySystemName(model.AwsRegion);
        AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(awsCredentials, region);
        var message = new Amazon.BedrockRuntime.Model.Message();
        message.Content = new List<ContentBlock> { new ContentBlock { Text = prompt } };
        message.Role = ConversationRole.User;
        ConverseRequest request = new ConverseRequest
        {
          ModelId = model.AwsModelId,
          Messages = new List<Amazon.BedrockRuntime.Model.Message> { message }
        };
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var converseResponse = await client.ConverseAsync(request);
        stopwatch.Stop();
        return new LlmResults(
          converseResponse.Output.Message.Content.First().Text,
          model.Name ?? "unknown",
          stopwatch.ElapsedMilliseconds);
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("awsRegion", model.AwsRegion);
        exception.Data.Add("modelId", model.AwsModelId);
        throw exception;
      }
    }

    public static async Task<LlmResults> GetFromGroq(Model model, string prompt)
    {
      var apiKey = model.GroqApiKey;
      if (String.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Provider is set to Groq but 'GroqApiKey' is empty in appsettings.");
      var groqModelId = model.GroqModelId;
      if (String.IsNullOrWhiteSpace(groqModelId))
        throw new Exception("Provider is set to Groq but 'GroqModelId' is empty in appsettings.");
      try
      {
        var groqClient = new GroqClient(apiKey, groqModelId);
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var text = await groqClient.CreateChatCompletionAsync(
          new GroqSharp.Models.Message { Role = MessageRoleType.User, Content = prompt });
        stopwatch.Stop();
        if (text == null)
          throw new Exception("Groq response is null.");
        return new LlmResults(
          text,
          model.Name ?? "unknown",
          stopwatch.ElapsedMilliseconds);
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Groq.", ex);
        exception.Data.Add("groqModelId", groqModelId);
        throw exception;
      }
    }

    public static async Task<LlmResults> GetFromOllama(Model model, string prompt)
    {
      var ollamaUrl = model.OllamaUrl;
      if (String.IsNullOrWhiteSpace(ollamaUrl))
        throw new Exception("Provider is set to Ollama but 'OllamaUrl' is empty in appsettings.");
      var ollamaModelId = model.OllamaModelId;
      if (String.IsNullOrWhiteSpace(ollamaModelId))
        throw new Exception("Provider is set to Ollama but 'OllamaModelId' is empty in appsettings.");
      try
      {
        IChatClient client = new OllamaChatClient(new Uri(ollamaUrl), ollamaModelId);
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await client.CompleteAsync(
          [
              new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, prompt)
          ]);
        stopwatch.Stop();
        var text = result.Message.Text;
        if (text == null)
          throw new Exception("Ollama response is null.");
        return new LlmResults(
          text,
          model.Name ?? "unknown",
          stopwatch.ElapsedMilliseconds);
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Ollama.", ex);
        exception.Data.Add("ollamaUrl", ollamaUrl);
        exception.Data.Add("ollamaModelId", ollamaModelId);
        throw exception;
      }
    }

    public static async Task<LlmResults> GetFromGemini(Model model, string prompt)
    {
      var geminiApiKey = model.GeminiApiKey;
      if (String.IsNullOrWhiteSpace(geminiApiKey))
        throw new Exception("Provider is set to Gemini but 'GeminiApiKey' is empty in appsettings.");
      var geminiModel = model.GeminiModel;
      if (String.IsNullOrWhiteSpace(geminiModel))
        throw new Exception("Provider is set to Gemini but 'geminiModel' is empty in appsettings.");
      try
      {
        var googleAI = new GoogleAI(apiKey: geminiApiKey);
        var request = googleAI.GenerativeModel(model: geminiModel);
        var generationConfig = new GenerationConfig();
        if (geminiModel.Contains("2.5"))
        {
          generationConfig.ThinkingConfig = new ThinkingConfig()
          {
            ThinkingBudget = 0
          };
        }
        ;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var response = await request.GenerateContent(prompt, generationConfig);
        stopwatch.Stop();
        if (response == null)
          throw new Exception("Gemini response is null.");
        try
        {
          return new LlmResults(
            response.Text ?? string.Empty,
            model.Name ?? "unknown",
            stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
          Exception exception = new("Error in Gemini response.", ex);
          exception.Data.Add("geminiModel", geminiModel);
          exception.Data.Add("PromptFeedback", response.PromptFeedback);
          throw exception;
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Gemini.", ex);
        exception.Data.Add("geminiModel", geminiModel);
        throw exception;
      }
    }
    public static async Task<LlmResults> GetFromOpenAI(Model model, string prompt)
    {
      var openAiApiKey = model.OpenAiApiKey;
      var openAiModel = model.OpenAiModel;
      if (String.IsNullOrWhiteSpace(openAiModel))
        throw new Exception("Provider is set to OpenAi but 'OpenAiModel' is empty in appsettings.");
      try
      {
        OpenAIAuthentication openAiAuthentication = new(openAiApiKey);
        string? openAiResourceName = model.OpenAiResourceName;
        string? openAiVersion = model.OpenAiVersion;
        OpenAIClientSettings openAiClientSettings;
        if (openAiResourceName != null)
        {
          if (openAiVersion != null)
            openAiClientSettings = new(openAiResourceName, openAiVersion);
          else
            openAiClientSettings = new(openAiResourceName);
        }
        else
          openAiClientSettings = new();

        var openAiClient = new OpenAIClient(openAiAuthentication, openAiClientSettings);
        var messages = new List<OpenAI.Chat.Message>
        {
          new OpenAI.Chat.Message(OpenAI.Role.User, prompt)
        };
        var modelInstance = new OpenAI.Models.Model(openAiModel);
        var chatRequest = new ChatRequest(messages, modelInstance);

        ChatResponse response;
        try
        {
          Stopwatch stopwatch = new Stopwatch();
          stopwatch.Start();
          response = await openAiClient.ChatEndpoint.GetCompletionAsync(chatRequest);
          stopwatch.Stop();
          return new LlmResults(
            response.FirstChoice.Message,
            model.Name ?? "unknown",
            stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
          var newEx = new Exception("Error getting response form OpenAI.", ex);
          newEx.Data.Add("openAiResourceName", openAiResourceName);
          newEx.Data.Add("openAiModel", openAiModel);
          throw newEx;
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing OpenAI.", ex);
        throw exception;
      }
    }
  }
}
