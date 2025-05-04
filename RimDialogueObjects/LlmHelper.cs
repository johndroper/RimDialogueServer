using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Config;
using GroqSharp.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using RimDialogue.Core;
using RimDialogueObjects.Templates;
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

    public async static Task<string> GenerateResponse(string prompt, Config config)
    {
      string? text = null;
      try
      {
        text = await GetResults(config, prompt);
        //****Remove everything between the start <think> tag and the end </think> tag ******
        if (config.RemoveThinking)
          text = Regex.Replace(text, "<think>(.|\n)*?</think>", "").Trim();
        return text;
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred fetching results.", ex);
        exception.Data.Add("prompt", prompt);
        throw exception;
      }
    }

    public static string Generate<DataT, TemplateT>(
      Config config,
      PawnData initiator,
      PawnData recipient,
      DataT dialogueData,
      out bool wasTruncated,
      out string prompt) where DataT : RimDialogue.Core.InteractionData.DialogueData where TemplateT : DialoguePromptTemplate<DataT>, new()
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
        prompt = promptTemplate.TransformText();
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

    public static string Generate<DataT, TemplateT>(
      Config config,
      PawnData initiator,
      PawnData recipient,
      DataT dialogueData,
      PawnData? targetData,
    out bool wasTruncated) where DataT : RimDialogue.Core.InteractionData.DialogueTargetData where TemplateT : DialogueTargetTemplate<DataT>, new()
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
        prompt = promptTemplate.TransformText();
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



    public static async Task<string> GetResults(Config config, string prompt)
    {
      var provider = config.Provider?.ToUpper();
      try
      {
        switch (provider)
        {
          case "AWS":
            return await GetFromAws(config, prompt);
          case "OLLAMA":
            return await GetFromOllama(config, prompt);
          case "OPENAI":
            return await GetFromOpenAI(config, prompt);
          case "GROQ":
            return await GetFromGroq(config, prompt);
          case "GEMINI":
            return await GetFromGemini(config, prompt);
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
      Config config,
      string prompt)
    {
      try
      {
        BasicAWSCredentials awsCredentials = new(config.AwsKey, config.AwsSecret);
        var region = Amazon.RegionEndpoint.GetBySystemName(config.AwsRegion);
        AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(awsCredentials, region);
        var message = new Amazon.BedrockRuntime.Model.Message();
        message.Content = new List<ContentBlock> { new ContentBlock { Text = prompt } };
        message.Role = ConversationRole.User;
        ConverseRequest request = new ConverseRequest
        {
          ModelId = config.AwsModelId,
          Messages = new List<Amazon.BedrockRuntime.Model.Message> { message },
          InferenceConfig = new InferenceConfiguration
          {
            MaxTokens = config.MaxOutputWords,
          }
        };
        var converseResponse = await client.ConverseAsync(request);
        return converseResponse.Output.Message.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("awsRegion", config.AwsRegion);
        exception.Data.Add("modelId", config.AwsModelId);
        throw exception;
      }
    }

    //public static async Task<string> GetFromAzureOpenAi(IConfiguration configuration, string prompt)
    //{
    //  var openAiUri = configuration["OpenAiUri"];
    //  if (String.IsNullOrWhiteSpace(openAiUri))
    //    throw new Exception("Provider is set to OpenAi but 'OpenAiUri' is empty in appsettings.");
    //  var openAiApiKey = configuration["OpenAiApiKey"];
    //  if (String.IsNullOrWhiteSpace(openAiApiKey))
    //    throw new Exception("Provider is set to OpenAi but 'OpenAiApiKey' is empty in appsettings.");
    //  var openAiDeployment = configuration["OpenAiDeployment"];
    //  if (String.IsNullOrWhiteSpace(openAiDeployment))
    //    throw new Exception("Provider is set to OpenAi but 'OpenAiDeployment' is empty in appsettings.");
    //  try
    //  {
    //    AzureOpenAIClient azureClient = new(
    //      new Uri(openAiUri),
    //      new ApiKeyCredential(openAiApiKey));
    //    ChatClient chatClient = azureClient.GetChatClient(openAiDeployment);
    //    var results = await chatClient.CompleteChatAsync(new OpenAI.Chat.UserChatMessage(prompt));
    //    return results.Value.Content.First().Text;
    //  }
    //  catch (Exception ex)
    //  {
    //    Exception exception = new("Error accessing Azure.", ex);
    //    exception.Data.Add("openAiUri", openAiUri);
    //    exception.Data.Add("openAiDeployment", openAiDeployment);
    //    throw exception;
    //  }
    //}

    public static async Task<string> GetFromGroq(Config config, string prompt)
    {
      var apiKey = config.GroqApiKey;
      if (String.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Provider is set to Groq but 'GroqApiKey' is empty in appsettings.");
      var groqModelId = config.GroqModelId;
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

    public static async Task<string> GetFromOllama(Config config, string prompt)
    {
      var ollamaUrl = config.OllamaUrl;
      if (String.IsNullOrWhiteSpace(ollamaUrl))
        throw new Exception("Provider is set to Ollama but 'OllamaUrl' is empty in appsettings.");
      var ollamaModelId = config.OllamaModelId;
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

    public static async Task<string> GetFromGemini(Config config, string prompt)
    {
      var geminiApiKey = config.GeminiApiKey;
      if (String.IsNullOrWhiteSpace(geminiApiKey))
        throw new Exception("Provider is set to Gemini but 'GeminiApiKey' is empty in appsettings.");
      var geminiUrl = config.GeminiUrl;
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
        try
        {
          if (response.Candidates == null)
          {
            var ex = new Exception("response.Candidates is null");
            throw ex;
          }
          var candidate = response.Candidates.FirstOrDefault();
          if (candidate == null)
          {
            var ex = new Exception("Gemini response has no candidates.");
            throw ex;
          }
          try
          {
            var part = candidate.Content.Parts.FirstOrDefault();
            if (part == null)
            {
              var ex = new Exception("Gemini response has no parts.");
              throw ex;
            }
            var text = part.Text;
            if (text == null)
              throw new Exception("Gemini response is null.");
            return text;
          }
          catch (Exception ex)
          {
            Exception exception = new("Error in Gemini candidates.", ex);
            exception.Data.Add("SafetyRatings", candidate.SafetyRatings);
            exception.Data.Add("FinishReason", candidate.FinishReason);
            throw exception;
          }
        }
        catch (Exception ex)
        {
          Exception exception = new("Error in Gemini response.", ex);
          exception.Data.Add("PromptFeedback", response.PromptFeedback);
          throw exception;
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Gemini.", ex);
        exception.Data.Add("geminiUrl", geminiUrl);
        throw exception;
      }
    }
    public static async Task<string> GetFromOpenAI(Config config, string prompt)
    {
      var openAiApiKey = config.OpenAiApiKey;
      var openAiModel = config.OpenAiModel;
      if (String.IsNullOrWhiteSpace(openAiModel))
        throw new Exception("Provider is set to OpenAi but 'OpenAiModel' is empty in appsettings.");
      try
      {
        OpenAIAuthentication openAiAuthentication = new(openAiApiKey);
        string? openAiResourceName = config.OpenAiResourceName;
        string? openAiVersion = config.OpenAiVersion;
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
          new OpenAI.Chat.Message(Role.User, prompt)
        };
        Model model = new Model(openAiModel);
        var chatRequest = new ChatRequest(messages, model);

        ChatResponse response;
        try
        {
          response = await openAiClient.ChatEndpoint.GetCompletionAsync(chatRequest);
          return response.FirstChoice.Message;

        }
        catch(Exception ex)
        {
          var newEx = new Exception("Error getting response form OpenAI.", ex);
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
