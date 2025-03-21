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
using RimDialogue.Core.InteractionData;
using RimDialogueObjects;
using RimDialogueObjects.Templates;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
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

    public void InitLog(string? action = null, [CallerMemberName] string? callerName = null)
    {
      if (action == null)
        action = callerName;
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

    [NonAction]
    public async Task<IActionResult> ProcessDialogue<DataT, TemplateT>(
      string action,
      string initiatorJson,
      string recipientJson,
      string dataJson)
    where DataT : RimDialogue.Core.InteractionData.DialogueData
    where TemplateT : DialoguePromptTemplate<DataT>, new()
    {
      InitLog(action);
      try
      {
        if (dataJson == null)
          return new BadRequestResult();
        if (initiatorJson == null)
          throw new Exception("initiatorJson is null.");
        if (recipientJson == null)
          throw new Exception("recipientJson is null.");

        string? ipAddress = GetIp();
        Log(ipAddress, dataJson);
        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        DataT? dialogueData = default(DataT);
        PawnData? initiator = null;
        PawnData? recipient = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<DataT>(dataJson);
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
          exception.Data.Add("dataJson", dataJson);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = LlmHelper.Generate<DataT, TemplateT>(
          config,
          initiator,
          recipient,
          dialogueData,
          out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await LlmHelper.GenerateResponse(prompt, Configuration);
        Log(text);
        var dialogueResponse = LlmHelper.SerializeResponse(text, Configuration, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {text.Length} characters.");
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



    [NonAction]
    public async Task<IActionResult> ProcessTargetDialogue<DataT, TemplateT>(
      string action, 
      string initiatorJson, 
      string recipientJson, 
      string dataJson, 
      string? targetJson) 
        where DataT : DialogueTargetData
        where TemplateT : DialogueTargetTemplate<DataT>, new()
    {
      InitLog(action);
      try 
      {
        if (dataJson == null)
          return new BadRequestResult();
        if (initiatorJson == null)
          throw new Exception("initiatorJson is null.");
        if (recipientJson == null)
          throw new Exception("recipientJson is null.");

        string? ipAddress = GetIp();
        Log(ipAddress, dataJson);
        var config = Configuration.GetSection("Options").Get<Config>();
        if (config == null)
          throw new Exception("config is null.");
        if (IsOverRateLimit(config, ipAddress))
          return new JsonResult(new DialogueResponse { RateLimited = true });
        DataT? dialogueData = default(DataT);
        PawnData? initiator = null;
        PawnData? recipient = null;
        PawnData? target = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<DataT>(dataJson);
          if (dialogueData == null)
            throw new Exception("DialogueData is null.");
          initiator = JsonConvert.DeserializeObject<PawnData>(initiatorJson);
          if (initiator == null)
            throw new Exception("Initiator is null.");
          recipient = JsonConvert.DeserializeObject<PawnData>(recipientJson);
          if (recipient == null)
            throw new Exception("Recipient is null.");
          if (targetJson != null)
            target = JsonConvert.DeserializeObject<PawnData>(targetJson);
          Log("Deserialized dialogueData.");
        }
        catch (Exception ex)
        {
          Exception exception = new("An error occurred deserializing JSON.", ex);
          exception.Data.Add("initiatorJson", initiatorJson);
          exception.Data.Add("recipientJson", recipientJson);
          exception.Data.Add("dataJson", dataJson);
          throw exception;
        }
        //******Prompt Generation******
        string prompt = LlmHelper.Generate<DataT, TemplateT>(
          config, 
          initiator, 
          recipient, 
          dialogueData, 
          target, 
          out bool inputTruncated);
        Log(prompt, $"inputTruncated: {inputTruncated}");
        //******Response Generation******
        string? text = await LlmHelper.GenerateResponse(prompt, Configuration);
        Log(text);
        var dialogueResponse = LlmHelper.SerializeResponse(text, Configuration, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {text.Length} characters.");
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
    public async Task<IActionResult> RecentIncidentChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataIncident, ChitChatRecentIncidentTemplate>("RecentIncidentChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }

    public async Task<IActionResult> RecentBattleChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataBattle, ChitChatBattleTemplate>("RecentBattleChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    public async Task<IActionResult> GameConditionChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
       return await ProcessDialogue<DialogueDataCondition, ChitChatGameConditionTemplate>("GameConditionChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    public async Task<IActionResult> MessageChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataMessage, ChitChatMessageTemplate>("MessageChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }

    public async Task<IActionResult> AlertChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueDataAlert, ChitChatAlertTemplate>("AlertChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }

    public async Task<IActionResult> IdeologyChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<RimDialogue.Core.InteractionData.DialogueData, ChitChatIdeologyTemplate>("IdeologyChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    public async Task<IActionResult> SkillChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataSkill, ChitChatSkillTemplate>("SkillsChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    public async Task<IActionResult> ColonistChitchat(string initiatorJson, string recipientJson, string chitChatJson, string? targetJson)
    {
      return await ProcessTargetDialogue<DialogueTargetData, ChitChatColonistTemplate>("ColonistChitchat", initiatorJson, recipientJson, chitChatJson, targetJson);
    }
    public async Task<IActionResult> HealthChitchat(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<DialogueDataHealth, ChitChatHealthTemplate>("HealthChitchat", initiatorJson, recipientJson, chitChatJson);
    }

    public async Task<IActionResult> Dialogue(string initiatorJson, string recipientJson, string chitChatJson)
    {
      return await ProcessDialogue<RimDialogue.Core.InteractionData.DialogueData, ChitChatDialogueTemplate>("DialogueChitchat", initiatorJson, recipientJson, chitChatJson);
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
        RimDialogue.Core.DialogueData? dialogueData = null;
        //******Deserialization******
        try
        {
          dialogueData = JsonConvert.DeserializeObject<RimDialogue.Core.DialogueData>(dialogueDataJSON);
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
        string? text = await LlmHelper.GenerateResponse(prompt, Configuration);
        Log(text);
        //****Remove everything between the start <think> tag and the end </think> tag ******
        if (Configuration.GetValue("RemoveThinking", false))
          text = Regex.Replace(text, "<think>(.|\n)*?</think>", "").Trim();
        //******Response Serialization******
        var dialogueResponse = LlmHelper.SerializeResponse(text, Configuration, out bool outputTruncated);
        if (outputTruncated)
          Log($"Response was truncated. Original length was {text.Length} characters.");
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
