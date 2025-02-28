using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using GroqSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI.Chat;
using RimDialogue.Core;
using RimDialogueObjects;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace RimDialogueLocal.Controllers
{
  public class HomeController(IConfiguration Configuration, IMemoryCache memoryCache) : Controller
  {
    private DateTime? _startTime;
    private DateTime? _lastLog = null;
    private StreamWriter? log;

    public override void OnActionExecuted(ActionExecutedContext context)
    {
      base.OnActionExecuted(context);
      if (log != null)
      {
        if (_startTime != null)
        log.WriteLine($"End of log. Total time taken: {DateTime.Now - _startTime}");
        log.Flush();
        log.Close();
      }
    }

    public void InitLog(string action)
    {
      if (log == null && Configuration["LoggingEnabled"]?.ToLower() == "true")
      {
        _startTime = DateTime.Now;
        if (!Directory.Exists("Output"))
          Directory.CreateDirectory("Output");
        log = System.IO.File.CreateText($"Output\\{action}-log-{DateTime.Now.Ticks}.txt");
        log.AutoFlush = true;
      }
    }

    public void Log(params string?[] data)
    {
      if (log == null)
        return;
      try
      {
        if (!Directory.Exists("Output"))
          Directory.CreateDirectory("Output");
        TimeSpan timeSinceLastLog = TimeSpan.Zero;
        if (_lastLog != null)
        {
          timeSinceLastLog = (TimeSpan)(DateTime.Now - _lastLog);
          log.WriteLine($"Time since last log: {timeSinceLastLog}");
        }
        foreach (var item in data)
        {
          if (item != null)
            log.WriteLine(item);
        }
        _lastLog = DateTime.Now;
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

    private bool IsOverRateLimit(Config config, string ipAddress) 
    {
      var rateLimit = RequestRate.CheckRateLimit(ipAddress,
        config.RateLimit,
        config.RateLimitCacheMinutes,
        config.MinRateLimitRequestCount,
        memoryCache,
        out float? rate);

      if (rateLimit)
        Log("Rate limited.", $"Rate {rate} > {config.RateLimit}");

      return rateLimit;
    }

    private string GetIp()
    {
      string? ipAddress = this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
      if (ipAddress == null)
        throw new Exception("IP Address is null.");
      return ipAddress;
    }

    private DialogueResponse SerializeResponse(string text, out bool outputTruncated)
    {
      //******Response Serialization******
      DialogueResponse? dialogueResponse = null;
      try
      {
        dialogueResponse = new DialogueResponse();
        int maxResponseLength = Configuration.GetValue<int>("MaxResponseLength", 5000);
        if (text.Length > maxResponseLength)
        {
          outputTruncated = true;
          Log($"Response truncated to {maxResponseLength} characters. Original length was {text.Length} characters.");
          text = text.Substring(0, maxResponseLength) + "...";
        }
        else
          outputTruncated = false;
        dialogueResponse.text = text;
        return dialogueResponse;
      }
      catch (Exception ex)
      {
        Exception exception = new("Error creating DialogueResponse.", ex);
        exception.Data.Add("text", text);
        throw exception;
      }
    }



    public async Task<string> GenerateResponse(string prompt)
    {
      string? text = null;
      try
      {
        text = await LlmHelper.GetResults(Configuration, prompt);
        Log(text);
        //****Remove everything between the start <think> tag and the end </think> tag ******
        if (Configuration.GetValue("RemoveThinking", false))
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

    public void LogException(Exception ex)
    {
      StringBuilder sb = new();
      foreach (var innerEx in ExceptionHelper.GetExceptionStack(ex))
      {
        sb.AppendLine(innerEx.Message);
        sb.AppendLine(innerEx.StackTrace);
        sb.AppendLine("Data:");
        foreach (var key in innerEx.Data.Keys)
        {
          sb.AppendLine($"{key}: {innerEx.Data[key]}");
        }
      }
      Log(sb.ToString());
    }

    [HttpPost]
    public async Task<IActionResult> GetChitChatRecentIncident(string initiatorJson, string recipientJson, string chitChatRecentIncidentJSON)
    {
      InitLog("GetChitChatRecentIncident");
      try
      {
        if (chitChatRecentIncidentJSON == null)
          throw new Exception("jsonData is null.");
        if (initiatorJson == null)
          throw new Exception("initiatorJson is null.");
        if (recipientJson == null)
          throw new Exception("recipientJson is null.");

        string? ipAddress = GetIp();
        Log(ipAddress, chitChatRecentIncidentJSON);
        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        ChitChatRecentIncidentData? dialogueData = null;
        PawnData? initiator = null;
        PawnData? recipient = null;
        //******Deserialization******
        try                                                                                                                                                 
        {
          dialogueData = JsonConvert.DeserializeObject<ChitChatRecentIncidentData>(chitChatRecentIncidentJSON);
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          initiator = JsonConvert.DeserializeObject<PawnData>(initiatorJson);
          if (initiator == null)
            throw new Exception("Initiator is null.");
          recipient = JsonConvert.DeserializeObject<PawnData>(recipientJson);
          if (recipient == null)
            throw new Exception("Recipient is null.");
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("recipientJson", recipientJson);
          exception.Data.Add("chitChatRecentIncidentJSON", chitChatRecentIncidentJSON);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = ChitChatRecentIncidentTemplate.Generate(config, initiator, recipient, dialogueData, out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await GenerateResponse(prompt);
        var dialogueResponse = SerializeResponse(text, out bool outputTruncated);
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          text.Length,
          inputTruncated,
          outputTruncated,
          null);
        Log("Metrics updated.");
        return new JsonResult(dialogueResponse);
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    [HttpPost]
    public async Task<IActionResult> GetDialogue(string dialogueDataJSON)
    {
      try
      {
        if (dialogueDataJSON == null)
          throw new Exception("dialogueDataJSON is null.");
        InitLog("GetDialogue");
        string? ipAddress = GetIp();
        Log(ipAddress, dialogueDataJSON);
        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        DialogueData? dialogueData = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<DialogueData>(dialogueDataJSON);
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("dialogueDataJSON", dialogueDataJSON);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = PromptTemplate.Generate(config, dialogueData, out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await GenerateResponse(prompt);
        //****Remove everything between the start <think> tag and the end </think> tag ******
        if (Configuration.GetValue("RemoveThinking", false))
          text = Regex.Replace(text, "<think>(.|\n)*?</think>", "").Trim();
        var dialogueResponse = SerializeResponse(text, out bool outputTruncated);
        //******Metrics******
        Metrics.AddRequest(
          this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString(),
          prompt.Length,
          text.Length,
          inputTruncated,
          outputTruncated,
          null);
        Log("Metrics updated.");
        return new JsonResult(dialogueResponse);
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }
  }
}
