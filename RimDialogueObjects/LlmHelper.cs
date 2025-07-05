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

    public async static Task<string> GenerateResponse(string prompt, Config config, string modelName)
    {
      string? text = null;
      try
      {
        text = await GetResults(config, prompt, modelName);
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

    public static async Task<string> GetResults(Config config, string prompt, string modelName)
    {

      var model = config.Models.FirstOrDefault(m => m.Name == modelName) ;
      if (model == null)
        model = config.Models[0];
    
      try
      {
        switch (model.Provider)
        {
          case "AWS":
            return await GetFromAws(model, config.MaxOutputWords, prompt);
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

    public static async Task<string> GetFromAws(
      Model model,
      int maxOutputWords,
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
          Messages = new List<Amazon.BedrockRuntime.Model.Message> { message },
          InferenceConfig = new InferenceConfiguration
          {
            MaxTokens = maxOutputWords,
          }
        };
        var converseResponse = await client.ConverseAsync(request);
        return converseResponse.Output.Message.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("awsRegion", model.AwsRegion);
        exception.Data.Add("modelId", model.AwsModelId);
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

    public static async Task<string> GetFromGroq(Model model, string prompt)
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

    public static async Task<string> GetFromOllama(Model model, string prompt)
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

    public static async Task<string> GetFromGemini(Model model, string prompt)
    {
      var geminiApiKey = model.GeminiApiKey;
      if (String.IsNullOrWhiteSpace(geminiApiKey))
        throw new Exception("Provider is set to Gemini but 'GeminiApiKey' is empty in appsettings.");
      var geminiUrl = model.GeminiUrl;
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
    public static async Task<string> GetFromOpenAI(Model model, string prompt)
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
          new OpenAI.Chat.Message(Role.User, prompt)
        };
        var modelInstance = new OpenAI.Models.Model(openAiModel);
        var chatRequest = new ChatRequest(messages, modelInstance);

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
