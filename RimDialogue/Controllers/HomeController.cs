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

namespace RimDialogue.Controllers
{
  public class HomeController(IConfiguration Configuration, IMemoryCache memoryCache) : Controller
  {
    private readonly BasicAWSCredentials awsCredentials = new(Configuration["AwsKey"], Configuration["AwsSecret"]);

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
      string? text = null;
      try
      {
        AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(awsCredentials, Amazon.RegionEndpoint.USEast1);
        Message message = new Message();
        message.Content = new List<ContentBlock> { new ContentBlock { Text = prompt } };
        message.Role = ConversationRole.User;
        ConverseRequest request = new ConverseRequest
        {
          ModelId = "arn:aws:bedrock:us-east-1:370824788989:inference-profile/us.meta.llama3-2-3b-instruct-v1:0",
          Messages = new List<Message> { message }
        };
        var converseResponse = await client.ConverseAsync(request);
        text = converseResponse.Output.Message.Content.First().Text;
        if (text == null)
          throw new Exception("No text returned.");
      }
      catch(Exception ex)
      {
        Exception exception = new("Error accessing Bedrock.", ex);
        exception.Data.Add("prompt", prompt);
        throw exception;
      }
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
