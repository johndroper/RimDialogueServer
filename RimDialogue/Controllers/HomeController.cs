using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using GroqSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OpenAI.Chat;
using RimDialogue.Core;
using RimDialogueObjects;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Text;
using System.Text.RegularExpressions;

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

    public void InitLog()
    {
      if (log == null && Configuration["LoggingEnabled"]?.ToLower() == "true")
      {
        _startTime = DateTime.Now;
        if (!Directory.Exists("Output"))
          Directory.CreateDirectory("Output");
        log = System.IO.File.CreateText($"Output\\data-log-{DateTime.Now.Ticks}.txt");
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
    public async Task<IActionResult> GetDialogue(string dialogueDataJSON)
    {
      if (dialogueDataJSON == null)
        throw new Exception("dialogueDataJSON is null.");
      InitLog();
      try
      {
        DialogueData? dialogueData = null;
        string? ipAddress = this.Request.HttpContext.Connection?.RemoteIpAddress?.ToString();
        if (ipAddress == null)
          throw new Exception("IP Address is null.");
        Log(ipAddress, dialogueDataJSON);

        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");

        if (RequestRate.CheckRateLimit(ipAddress,
          config.RateLimit,
          config.RateLimitCacheMinutes,
          config.MinRateLimitRequestCount,
          memoryCache,
          out float? rate))
        {
          Log("Rate limited.", $"Rate {rate} > {config.RateLimit}");
          return new JsonResult(new DialogueResponse { RateLimited = true });
        }

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
        string? text = null;
        try
        {
          text = await LlmHelper.GetResults(Configuration, prompt);
          Log(text);
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred fetching results.", ex);
          exception.Data.Add("prompt", prompt);
          throw exception;
        }

        //****Remove everything between the start <think> tag and the end </think> tag ******
        if (Configuration.GetValue("RemoveThinking", false))
          text = Regex.Replace(text, "<think>(.|\n)*?</think>", "").Trim();

        //******Response Serialization******
        DialogueResponse? dialogueResponse = null;
        bool outputTruncated = false;
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
          dialogueResponse.text = text;
        }
        catch (Exception ex)
        {
          Exception exception = new("Error creating DialogueResponse.", ex);
          exception.Data.Add("text", text);
          throw exception;
        }

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
        throw;
      }
    }
  }
}
