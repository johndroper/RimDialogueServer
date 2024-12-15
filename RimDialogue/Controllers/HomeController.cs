using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Azure.AI.OpenAI;
using GroqSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.ClientModel;
using RimDialogue.Core;
using RimDialogueObjects;
using static System.Net.Mime.MediaTypeNames;

namespace RimDialogueLocal.Controllers
{
  public class HomeController(IConfiguration Configuration, IMemoryCache memoryCache) : Controller
  {
    public void Log(params string[] data)
    {
      //******Logging******
      try
      {
        if (Configuration["LoggingEnabled"]?.ToLower() == "true")
        {
          if (!Directory.Exists("Output"))
            Directory.CreateDirectory("Output");
          using (StreamWriter log = System.IO.File.CreateText($"Output\\data-log-{DateTime.Now.Ticks}.txt"))
          {
            foreach (var item in data)
              log.WriteLine(item);
          }
        }
      }
      catch (Exception ex)
      {
        Exception exception = new("Error logging data.", ex);
        throw exception;
      }
    }

    public IActionResult Index()
    {
      return View();
    }

    public async Task<IActionResult> GetDialogue(string dialogueDataJSON)
    {
      DialogueData? dialogueData = null;
      if (dialogueDataJSON == null)
        throw new Exception("dialogueDataJSON is null.");

      string? ipAddress = this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
      if (ipAddress == null) {
        throw new Exception("IP Address is null.");
      }

      var tier = Configuration.GetSection("Tier").Get<TierConfig>();
      if (tier == null)
        throw new Exception("Tier is null.");

      if (RequestRate.CheckRateLimit(ipAddress,
        tier.RateLimit,
        tier.RateLimitCacheMinutes,
        tier.MinRateLimitRequestCount,
        memoryCache))
        return new JsonResult(new DialogueResponse { RateLimited = true });

      //******Deserialization******
      try
      {
        dialogueData = JsonConvert.DeserializeObject<DialogueData>(dialogueDataJSON);
        if (dialogueData == null)
          throw new Exception("DialogueData is null.");
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred deserializing JSON.", ex);
        exception.Data.Add("dialogueDataJSON", dialogueDataJSON);
        throw exception;
      }

      //******Prompt Generation******
      string prompt = PromptTemplate.Generate(tier, dialogueData);

      //******Response Generation******
      string? text = null;
      try
      {
        text = await LlmHelper.GetResults(Configuration, prompt);
      }
      catch (Exception ex)
      {
        Exception exception = new("An error occurred fetching results.", ex);
        exception.Data.Add("prompt", prompt);
        throw exception;
      }

      //******Metrics******
      ServerMetrics.AddRequest(
        this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
        prompt.Length,
        text.Length);

      //******Response Serialization******
      DialogueResponse? dialogueResponse = null;
      try
      {
        dialogueResponse = new DialogueResponse();
        int maxResponseLength = Configuration.GetValue<int>("MaxResponseLength", 5000);
        if (text.Length > maxResponseLength)
        {
          Console.WriteLine($"Response truncated to {maxResponseLength} characters. Original length was {text.Length} characters.");
          text = text.Substring(0, maxResponseLength) + "...";
        }
        dialogueResponse.text = text;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error creating DialogueResponse.", ex);
        exception.Data.Add("text", text);
        throw exception;
      }

      Log(prompt, text);

      return new JsonResult(dialogueResponse);
    }
  }
}
