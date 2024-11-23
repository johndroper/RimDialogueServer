using RimDialogue.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.IO;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;
using GroqSharp.Models;
using GroqSharp;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace RimDialogue.Controllers
{
  public class HomeController(IConfiguration Configuration, IMemoryCache memoryCache) : Controller
  {
    public List<Conversation>? GetConversations(string key)
    {
      if (memoryCache.TryGetValue(key, out List<Conversation>? conversations))
        return conversations;
      else
        return null;
    }

    public void AddConversation(string key, Conversation conversation)
    {
      if (!memoryCache.TryGetValue(key, out List<Conversation>? conversations))
      {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        memoryCache.Set(key, new List<Conversation> { conversation }, cacheEntryOptions);
      }
      else if (conversations != null)
      {
        conversations = conversations.TakeLast(10).ToList();
        conversations.Add(conversation);
      }
    }

    private bool LoggingEnabled
    {
      get
      {
        return Configuration["LoggingEnabled"]?.ToLower() == "true";
      }
    }

    private readonly object _monitorObject = new object();

    public IActionResult Index()
    {
      return View();
    }

    private async Task<string> GetFromAws(string prompt)
    {
      try
      {
        var awsAccessKey = Configuration["AwsAccessKey"];
        if (String.IsNullOrWhiteSpace(awsAccessKey))
          throw new Exception("AWS Access Key is empty in appsettings.");
        var awsSecret = Configuration["AwsSecret"];
        if (String.IsNullOrWhiteSpace(awsSecret))
          throw new Exception("AWS Secret is empty in appsettings.");
        var awsRegion = Configuration["AwsRegion"];
        if (String.IsNullOrWhiteSpace(awsRegion))
          throw new Exception("AWS Region is empty in appsettings.");
        BasicAWSCredentials awsCredentials = new(awsAccessKey, awsSecret);
        var region = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
        AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(awsCredentials, region);
        Message message = new Message();
        message.Content = new List<ContentBlock> { new ContentBlock { Text = prompt } };
        message.Role = ConversationRole.User;
        ConverseRequest request = new ConverseRequest
        {
          ModelId = Configuration["AwsModelId"],
          Messages = new List<Message> { message }
        };
        var converseResponse = await client.ConverseAsync(request);
        return converseResponse.Output.Message.Content.First().Text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("prompt", prompt);
        throw exception;
      }
    }

    private async Task<string> GetFromOpenAi(string prompt)
    {
      var openAiUri = Configuration["OpenAiUri"];
      if (String.IsNullOrWhiteSpace(openAiUri))
        throw new Exception("Provider is set to OpenAi but 'OpenAiUri' is empty in appsettings.");
      var openAiApiKey = Configuration["OpenAiApiKey"];
      if (String.IsNullOrWhiteSpace(openAiApiKey))
        throw new Exception("Provider is set to OpenAi but 'OpenAiApiKey' is empty in appsettings.");
      var openAiDeployment = Configuration["OpenAiDeployment"];
      if (String.IsNullOrWhiteSpace(openAiDeployment))
        throw new Exception("Provider is set to OpenAi but 'OpenAiDeployment' is empty in appsettings.");

      AzureOpenAIClient azureClient = new(
        new Uri(openAiUri),
        new ApiKeyCredential(openAiApiKey));
      ChatClient chatClient = azureClient.GetChatClient(openAiDeployment);

      var results = await chatClient.CompleteChatAsync(new OpenAI.Chat.UserChatMessage(prompt));

      return results.Value.Content.First().Text;
    }

    private async Task<string> GetFromGroq(string prompt)
    {
      var apiKey = Configuration["GroqApiKey"];
      if (String.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Provider is set to Groq but 'GroqApiKey' is empty in appsettings.");
      var groqModelId = Configuration["GroqModelId"];
      if (String.IsNullOrWhiteSpace(groqModelId))
        throw new Exception("Provider is set to Groq but 'GroqModelId' is empty in appsettings.");
      var groqClient = new GroqClient(apiKey, groqModelId);
      var text = await groqClient.CreateChatCompletionAsync(
        new GroqSharp.Models.Message { Role = MessageRoleType.User, Content = prompt });
      if (text == null)
        throw new Exception("Groq response is null.");
      return text;
    }

    private async Task<string> GetFromOllama(string prompt)
    {
      var ollamaUrl = Configuration["OllamaUrl"];
      if (String.IsNullOrWhiteSpace(ollamaUrl))
        throw new Exception("Provider is set to Ollama but 'OllamaUrl' is empty in appsettings.");
      var ollamaModelId = Configuration["OllamaModelId"];
      if (String.IsNullOrWhiteSpace(ollamaModelId))
        throw new Exception("Provider is set to Ollama but 'OllamaModelId' is empty in appsettings.");
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
    private async Task<string> GetResults(string prompt)
    {
      switch(Configuration["provider"]?.ToUpper())
      {
        case "AWS":
          return await GetFromAws(prompt);
        case "OLLAMA":
          return await GetFromOllama(prompt);
        case "OPENAI":
          return await GetFromOpenAi(prompt);
        case null:
          throw new Exception($"Provider not set.");
        default:
          throw new Exception($"Unknown provider:'{Configuration["provider"]}'");
      }
    }

    public async Task<IActionResult> GetDialogue(string dialogueDataJSON)
    {
      string prompt = string.Empty;
      DialogueData? dialogueData = null;
      if (dialogueDataJSON == null)
        throw new Exception("dialogueDataJSON is null.");
      else
      {
        try
        {
          //rate limiting
          string? key = this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
          if (key == null)
            throw new Exception("Remote IP address is null.");
          if (memoryCache.TryGetValue(key, out RequestRate? requestRate) && requestRate != null)
          {
            if (requestRate.Count > 10)
            {
              float rateLimit;
              if (float.TryParse(Configuration["RateLimit"], out rateLimit))
              {
                var rate = requestRate.GetRate();
#if DEBUG
                Console.WriteLine("Rate: " + rate);
#endif
                if (rate > rateLimit)
                  return new JsonResult(new DialogueResponse { RateLimited = true });
              }
            }
            requestRate.Increment();
          }
          else
          {
            requestRate = new RequestRate(key);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
              .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            memoryCache.Set(key, requestRate, cacheEntryOptions);
          }
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred in rate limiting.", ex);
          throw exception;
        }

        try
        {
          dialogueData = JsonConvert.DeserializeObject<DialogueData>(dialogueDataJSON);
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("dialogueDataJSON", dialogueDataJSON);
          throw exception;
        }
        try
        {
          if (dialogueData == null)
            throw new Exception("dialogueData is null.");
          var initiatorConversations = GetConversations(dialogueData.clientId + dialogueData.initiatorThingID);
          var recipientConversations = GetConversations(dialogueData.clientId + dialogueData.recipientThingID);
          PromptTemplate promptTemplate = new(dialogueData, initiatorConversations, recipientConversations);
          prompt = promptTemplate.TransformText();
        }
        catch(Exception ex)
        {
          Exception exception = new("An error occurred generating prompt.", ex);
          exception.Data.Add("dialogueData", JsonConvert.SerializeObject(dialogueData));
          throw exception;
        }
      }

      string text = await GetResults(prompt);

      DialogueResponse? dialogueResponse = null;
      try
      {
        dialogueResponse  = new DialogueResponse();

        if (text.Length > 1000)
          text = text.Substring(0, 1000) + "...";
        if (dialogueData != null)
        {
          var conversation = new Conversation(dialogueData.initiatorFullName, dialogueData.recipientFullName, dialogueData.interaction, text);
          AddConversation(
            dialogueData.clientId + dialogueData.initiatorThingID,
            conversation);
          AddConversation(
            dialogueData.clientId + dialogueData.recipientThingID,
            conversation);
        }
        dialogueResponse.text = text;
      }
      catch(Exception ex)
      {
        Exception exception = new("Error creating DialogueResponse.", ex);
        exception.Data.Add("text", text);
        throw exception;
      }
      try
      {
        if (LoggingEnabled)
        {
          if (!Directory.Exists("Output"))
            Directory.CreateDirectory("Output");
          using (StreamWriter log = System.IO.File.CreateText($"Output\\data-log-{DateTime.Now.Ticks}.txt"))
          {
            log.WriteLine(dialogueDataJSON);
            log.WriteLine(prompt);
            log.WriteLine(text);
          }
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error logging data.", ex);
        throw exception;
      }
      return new JsonResult(dialogueResponse);
    }
  }
}
